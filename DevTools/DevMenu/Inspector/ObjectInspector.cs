using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using System.Reflection;
using System.Text.RegularExpressions;
using HarmonyLib;
using SALT.Extensions;
using SALT.Utils;
using UnityEngine;
// ReSharper disable MemberCanBeMadeStatic.Local

namespace SALT.DevTools.DevMenu
{
	internal class ObjectInspector
	{
		//+ CONSTANTS
		private const int LABEL_WIDTH = 200;
		
		//+ VARIABLES
		private static readonly Dictionary<Type, MethodInfo> DRAW_FUNCTIONS = new Dictionary<Type, MethodInfo>
		{
			//{ typeof(UnityEngine.Animations.AnimationPlayableOutput), TypeUtils.GetMethodBySearch(typeof(ObjectInspector), nameof(AnimationPlayableOutputField)) },
			//{ typeof(UnityEngine.Animations.AnimatorControllerPlayable),  TypeUtils.GetMethodBySearch(typeof(ObjectInspector), nameof(AnimatorControllerPlayableField)) },
			//{ typeof(UnityEngine.Playables.PlayableOutput),  TypeUtils.GetMethodBySearch(typeof(ObjectInspector), nameof(PlayableOutputField)) },
			//{ typeof(UnityEngine.Playables.PlayableGraph),  TypeUtils.GetMethodBySearch(typeof(ObjectInspector), nameof(PlayableGraphField)) },
			//{ typeof(UnityEngine.Playables.Playable),  TypeUtils.GetMethodBySearch(typeof(ObjectInspector), nameof(PlayableField)) },
			{ typeof(AnimatorControllerParameter),  TypeUtils.GetMethodBySearch(typeof(ObjectInspector), nameof(AnimatorControllerParameterField)) },
			{ typeof(AnimationEvent),  TypeUtils.GetMethodBySearch(typeof(ObjectInspector), nameof(AnimationEventField)) },
			{ typeof(Translation),  TypeUtils.GetMethodBySearch(typeof(ObjectInspector), nameof(TranslationField)) },
			{ typeof(TranslationCollection),  TypeUtils.GetMethodBySearch(typeof(ObjectInspector), nameof(TranslationCollectionField)) },
			{ typeof(Vector4), TypeUtils.GetMethodBySearch(typeof(ObjectInspector), nameof(Vector4Field)) },
			{ typeof(Vector3), TypeUtils.GetMethodBySearch(typeof(ObjectInspector), nameof(Vector3Field)) },
			{ typeof(Vector2), TypeUtils.GetMethodBySearch(typeof(ObjectInspector), nameof(Vector2Field)) },
			{ typeof(bool), TypeUtils.GetMethodBySearch(typeof(ObjectInspector), nameof(BoolField)) },
			{ typeof(Texture), TypeUtils.GetMethodBySearch(typeof(ObjectInspector), nameof(TextureField)) },
			{ typeof(Texture2D), TypeUtils.GetMethodBySearch(typeof(ObjectInspector), nameof(TextureField)) },
			{ typeof(Sprite), TypeUtils.GetMethodBySearch(typeof(ObjectInspector), nameof(SpriteField)) },
			{ typeof(Mesh), TypeUtils.GetMethodBySearch(typeof(ObjectInspector), nameof(MeshField)) },
			{ typeof(MusicTrack), TypeUtils.GetMethodBySearch(typeof(ObjectInspector), nameof(MusicTrackField)) },
		};

		private static readonly HashSet<string> TO_IGNORE = new HashSet<string>()
		{
			"transform",
			"gameObject",
			"tag",
			"name",
			"hideFlags"
		};
		private static readonly HashSet<System.Type> ENUMERABLE_IGNORE = new HashSet<System.Type>()
		{
			typeof (string)
		};
		private static readonly HashSet<System.Type> ALWAYS_SHOW_PROPS = new HashSet<System.Type>()
		{
		  typeof (GUISkin),
		  typeof (GUIStyle),
		  typeof (GUIStyleState),
		  typeof (RectOffset),
		  typeof (MeshFilter),
		  typeof (MeshRenderer)
		};
		private static readonly HashSet<System.Type> ALWAYS_HIDE_FIELDS = new HashSet<System.Type>()
		{
		  typeof (GUISkin),
		  typeof (GUIStyleState),
		  typeof (RectOffset),
		  typeof (MeshFilter),
		  typeof (MeshRenderer)
		};
		private static readonly Dictionary<System.Type, List<string>> ALWAYS_HIDE_SPECIFIC_FIELDS = new Dictionary<System.Type, List<string>>()
		{
			{
				typeof(AudioSource),
				new List<string>
				{
					"rolloffFactor",
					"minVolume",
					"maxVolume"
				}
			},
			{
				typeof(MeshFilter),
				new List<string>()
				{
					"mesh"
				}
			}
		};

		private readonly Dictionary<string, bool> arrayFolds = new Dictionary<string, bool>();
		private readonly Component component;
		private readonly ScriptableObject sObject;
		private readonly Material mat;
		private readonly Motion motion;
		private Vector2 scrollView;
		private int lastIndent;

		//+ CONSTRUCTOR
		internal ObjectInspector(Component component) => this.component = component;
		internal ObjectInspector(ScriptableObject sObject) => this.sObject = sObject;
		internal ObjectInspector(Material mat) => this.mat = mat;
		internal ObjectInspector(Motion motion) => this.motion = motion;

		//+ DRAWING
		// Draws the inspector
		internal void DrawInspector(ObjectComponent inspectComp = null)
		{
			TextClipping clipping = GUI.skin.label.clipping;
			GUI.skin.label.wordWrap = false;
			GUI.skin.label.clipping = TextClipping.Clip;
			if (sObject != null)
			{
				try
				{
					this.DrawComponent(sObject);
				}
				catch (Exception ex)
				{
					GUILayout.Label(ex.ParseTrace());
				}
			}
			else if (mat != null)
			{
				try
				{
					this.DrawComponent(mat);
				}
				catch (Exception ex)
				{
					GUILayout.Label(ex.ParseTrace());
				}
			}
			else if (motion != null)
			{
				try
				{
					this.DrawComponent(motion);
				}
				catch (Exception ex)
				{
					GUILayout.Label(ex.ParseTrace());
				}
			}
			else
			{
				if (component is RectTransform rectTransform)
					DrawRectTransform(rectTransform);
				else if(component is Transform transform)
					DrawTransform(transform);
				else
				{
					try
					{
						DrawComponent(objComp: inspectComp);
					}
					catch (Exception e)
					{
						GUILayout.Label(e.ParseTrace());
					}
				}
				GUI.skin.label.wordWrap = false;
				GUI.skin.label.clipping = clipping;
			}
		}

		// Draws a transform component
		internal void DrawTransform(Transform transform)
		{
			Vector3Field(transform.localPosition, "Position", false);
			Vector3Field(transform.localEulerAngles, "Rotation", false);
			Vector3Field(transform.localScale, "Scale", false);
			LayerField(transform.gameObject.layer, "Layer", false);
			StringField(string.IsNullOrWhiteSpace(transform.gameObject.scene.name) ? "Prefabs" : transform.gameObject.scene.name, "Scene", false);
			StringField(transform.gameObject.tag, "Tag", false);
		}

		// Draws a rect transform component
		internal void DrawRectTransform(RectTransform rectTransform)
		{
			DrawTransform(rectTransform);
			Vector2Field(rectTransform.anchoredPosition, "Anchored Position", false);
			Vector3Field(rectTransform.anchoredPosition3D, "Anchored Position 3D", false);
			Vector2Field(rectTransform.anchorMin, "Anchor Min", false);
			Vector2Field(rectTransform.anchorMax, "Anchor Max", false);
			Vector2Field(rectTransform.offsetMin, "Offset Min", false);
			Vector2Field(rectTransform.offsetMax, "Offset Max", false);
			Vector2Field(rectTransform.pivot, "Pivot", false);
			Vector2Field(rectTransform.sizeDelta, "Size Delta", false);
		}

		// Draws a unidentified component
		internal void DrawComponent(object @object = null, string extraFieldName = null, ObjectComponent objComp = null)
		{
			GUI.skin.scrollView.stretchHeight = true;
			scrollView = GUILayout.BeginScrollView(scrollView);
			object current = @object ?? component;

			// All Fields
			var fields = current.GetType().GetFields(AccessTools.all);
			bool hadField = false;
			if (!ALWAYS_HIDE_FIELDS.Contains(current.GetType()) && fields.Length != 0)
			{
				foreach (FieldInfo field in fields)
				{
					if (ALWAYS_HIDE_SPECIFIC_FIELDS.GetValueSafe(current.GetType()).Is(ahsf => ahsf != null && ahsf.Contains(field.Name))
						|| TO_IGNORE.Contains(field.Name)
						|| field.GetCustomAttribute<HideInInspector>() != null) continue;
					if (field.IsStatic) continue;

					bool isPrivate = !field.IsPublic;
					if (isPrivate && field.GetCustomAttribute<SerializeField>() == null) continue;

					string name = isPrivate ? $"[F] <i>{field.Name}</i>" : $"[F] {field.Name}";
					object value = field.GetValue(current);

					if (value != null)
						DrawField(field.FieldType, value, name, isPrivate, $"{(string.IsNullOrWhiteSpace(extraFieldName) ? extraFieldName + "." : string.Empty)}{field.Name}");
					else
						Field(string.Empty, name, isPrivate);
					hadField = true;
				}
			}

			// All Properties
			var properties = current.GetType().GetProperties(AccessTools.all);
			bool hadProp = false;
			if ((InspectorTab.showProperties || InspectorTab.showDebugInfo || ObjectInspector.ALWAYS_SHOW_PROPS.Contains(current.GetType())) && properties.Length != 0)
			{
				if (hadField)
					GUILayout.Space(20f);
				foreach (PropertyInfo field in properties)
				{
					if (ALWAYS_HIDE_SPECIFIC_FIELDS.GetValueSafe(current.GetType()).Is(ahsf => ahsf != null && ahsf.Contains(field.Name))
						|| TO_IGNORE.Contains(field.Name)) continue;
				
					if (!field.CanRead) continue;

					MethodInfo getMethod = field.GetGetMethod(true);

					if (getMethod == null)
					{
						Field(string.Empty, $"[P] <i>{field.Name}</i>", true);
						continue;
					}
					if (getMethod.IsStatic) continue;

					bool isPrivate = field.CanRead && !getMethod.IsPublic;
					if (!isPrivate || InspectorTab.showPrivateFields || InspectorTab.showDebugInfo)
					{
						string name = isPrivate ? $"[P] <i>{field.Name}</i>" : $"[P] {field.Name}"; //"~" + field.Name;
						object value = field.PropertyType.ToString();
						try { value = field.GetValue(current); }
						catch { /* Ignored */ }
						if (value != null)
							DrawField(field.PropertyType, value, name, isPrivate, $"{(string.IsNullOrWhiteSpace(extraFieldName) ? extraFieldName + "." : string.Empty)}{field.Name}");
						else
							Field(string.Empty, name, isPrivate);
						hadProp = true;
					}
				}
			}

			if (current is Material material)
			{
				DrawField(typeof(string[]), material.GetTexturePropertyNames(), "[P] propertyNames", false, $"{ (string.IsNullOrWhiteSpace(extraFieldName) ? extraFieldName + "." : string.Empty)}propertyNames");
				try
				{
					Dictionary<string, Texture> textureProperties = new Dictionary<string, Texture>();
					foreach (string prop in material.GetTexturePropertyNames())
						textureProperties.Add(prop, material.GetTexture(prop));
					DrawField(typeof(Dictionary<string, Texture>), textureProperties, "[P] textureProperties", false, $"{ (string.IsNullOrWhiteSpace(extraFieldName) ? extraFieldName + "." : string.Empty)}textureProperties");
				}
				catch (Exception ex)
				{
					GUILayout.Label(ex.ParseTrace());
				}
				hadProp = true;
			}

			GUILayout.EndScrollView();
			GUI.skin.scrollView.stretchHeight = false;
			if (objComp == null || hadProp || hadField)
				return;
			objComp.IsUnfolded = false;
		}

		// Draws a field based on it's type
		private void DrawField(Type type, object value, string label, bool isPrivate, string fieldName)
		{
			if (!DRAW_FUNCTIONS.ContainsKey(type))
			{
				if (type.IsArray && !ENUMERABLE_IGNORE.Contains(type))
					ArrayField(value, label, isPrivate, fieldName);
					//Field("Contains " + ((Array) value).Length + " entries", label, isPrivate);
				else if(typeof(IDictionary).IsAssignableFrom(type) && !ENUMERABLE_IGNORE.Contains(type))
					DictionaryField(value, label, isPrivate, fieldName);
				else if(typeof(IEnumerable).IsAssignableFrom(type) && !ENUMERABLE_IGNORE.Contains(type))
					EnumerableField(value, label, isPrivate, fieldName);
				else if (type.GetCustomAttribute<SerializableAttribute>() != null && !type.IsPrimitive)
					//SerializableTypeField(value, label, isPrivate, fieldName);
					Field(value, label, isPrivate);
				else
					Field(value, label, isPrivate);

				return;
			}
			
			if (value == null)
				DRAW_FUNCTIONS[type].Invoke(this, new object[3] { type.GetDefault(), label, isPrivate });
			else
				DRAW_FUNCTIONS[type].Invoke(this, new object[3] { value, label, isPrivate });
		}
		
		//+ HELPERS
		private void Field(object value, string label, bool isPrivate)
		{
			GUILayout.BeginHorizontal();
			DrawFieldLabel(label, isPrivate);
			GUILayout.Label(value?.ToString() ?? "null", GUI.skin.textField);//string.Empty, GUI.skin.textField);
			GUILayout.EndHorizontal();
		}

		private void EnumerableField(object value, string label, bool isPrivate, string fieldName)
		{
			if (value is Transform transform)
			{
				Field(transform, label, isPrivate);
				return;
			}
			else if (value is string str)
			{
				Field(str, label, isPrivate);
				return;
			}

			if (!this.arrayFolds.ContainsKey(fieldName))
				this.arrayFolds.Add(fieldName, false);
			GUILayout.BeginHorizontal();
			GUILayout.Space((float)(4 + this.lastIndent));
			if (GUILayout.Button(!this.arrayFolds[fieldName] || value == null ? "►" : "▼", GUI.skin.textField, GUILayout.ExpandWidth(false)))
				this.arrayFolds[fieldName] = !this.arrayFolds[fieldName];
			this.DrawFieldLabel(label, isPrivate, false);
			GUILayout.EndHorizontal();
			if (!this.arrayFolds[fieldName] || value == null)
				return;
			GUILayout.BeginVertical();
			IEnumerable enumerable = (IEnumerable)value;
			int num = 0;
			this.lastIndent += 16;
			foreach (object obj in enumerable)
			{
				if (obj != null)
					this.DrawField(obj.GetType(), obj, string.Format("Element {0}", (object)num), false, string.Format("{0}.{1}", (object)fieldName, (object)num));
				else
					this.DrawField(typeof(object), obj, string.Format("Element {0}", (object)num), false, string.Format("{0}.{1}", (object)fieldName, (object)num));
				++num;
			}
			this.lastIndent -= 16;
			GUILayout.EndVertical();
		}

		private void DictionaryField(object value, string label, bool isPrivate, string fieldName)
		{
			if (!this.arrayFolds.ContainsKey(fieldName))
				this.arrayFolds.Add(fieldName, false);
			GUILayout.BeginHorizontal();
			GUILayout.Space((float)(4 + this.lastIndent));
			if (GUILayout.Button(!this.arrayFolds[fieldName] || value == null ? "►" : "▼", GUI.skin.textField, GUILayout.ExpandWidth(false)))
				this.arrayFolds[fieldName] = !this.arrayFolds[fieldName];
			this.DrawFieldLabel(label, isPrivate, false);
			GUILayout.EndHorizontal();
			if (!this.arrayFolds[fieldName] || value == null)
				return;
			GUILayout.BeginVertical();
			IDictionary dictionary = (IDictionary)value;
			int num = 0;
			this.lastIndent += 16;
			foreach (var kv in dictionary.ToDictionary())
			{
				object key = kv.Key;
				object v = kv.Value;
				if (v != null)
					this.DrawField(v.GetType(), v, key.ToString(), false, string.Format("{0}.{1}", (object)fieldName, (object)num));
				else
					this.DrawField(typeof(object), v, key.ToString(), false, string.Format("{0}.{1}", (object)fieldName, (object)num));
				++num;
			}
			this.lastIndent -= 16;
			GUILayout.EndVertical();
		}

		private void ArrayField(object value, string label, bool isPrivate, string fieldName)
		{
			if (!arrayFolds.ContainsKey(fieldName))
				arrayFolds.Add(fieldName, false);

			GUILayout.BeginHorizontal();
			GUILayout.Space(4 + lastIndent);
			if (GUILayout.Button(arrayFolds[fieldName] ? InspectorTab.UNFOLDED : InspectorTab.FOLDED, GUI.skin.textField, GUILayout.ExpandWidth(false))) arrayFolds[fieldName] = !arrayFolds[fieldName];
			Array array = (Array)value;
			DrawFieldLabel(label + " [" + array.LongLength + "]", isPrivate, false);
			GUILayout.EndHorizontal();

			if (!arrayFolds[fieldName]) return;

			GUILayout.BeginVertical();

			int i = 0;
			lastIndent += 16;
			foreach (object obj in array)
			{
				DrawField(obj.GetType(), obj, $"Element {i}", false, $"{fieldName}.{i}");
				i++;
			}
			lastIndent -= 16;

			GUILayout.EndVertical();
		}

		private void SerializableTypeField(object value, string label, bool isPrivate, string fieldName)
		{
			if (!arrayFolds.ContainsKey(fieldName))
				arrayFolds.Add(fieldName, false);
			
			GUILayout.BeginHorizontal();
			GUILayout.Space(4 + lastIndent);
			if (GUILayout.Button(arrayFolds[fieldName] ? InspectorTab.UNFOLDED : InspectorTab.FOLDED, GUI.skin.textField, GUILayout.ExpandWidth(false))) arrayFolds[fieldName] = !arrayFolds[fieldName];
			DrawFieldLabel(label, isPrivate, false);
			GUILayout.EndHorizontal();

			if (!arrayFolds[fieldName]) return;
			
			GUILayout.BeginVertical();

			if (value != null)
			{
				lastIndent += 16;
				try
				{
					DrawComponent(value, fieldName);
				}
				catch (Exception e)
				{
					GUILayout.Label(label + "\n" + e.ParseTrace()); //GUILayout.Label(label + "\n" + e.Message + "\n" + e.StackTrace);
				}

				lastIndent -= 16;
			}
			else
			{
				GUILayout.Label(string.Empty, GUI.skin.textField);
			}

			GUILayout.EndVertical();
		}

		private void BoolField(object value, string label, bool isPrivate)
		{
			GUILayout.BeginHorizontal();
			DrawFieldLabel(label, isPrivate);
			GUILayout.Toggle(Convert.ToBoolean(value), string.Empty);
			GUILayout.EndHorizontal();
		}

		private void TextureField(object value, string label, bool isPrivate)
		{
			GUILayout.BeginHorizontal();
			this.DrawFieldLabel(label, isPrivate);
			GUILayout.FlexibleSpace();
			GUILayout.Label((Texture)value, GUILayout.MaxHeight(128f), GUILayout.MaxHeight(128f));
			GUILayout.FlexibleSpace();
			GUILayout.EndHorizontal();
		}

		private void SpriteField(object value, string label, bool isPrivate) => this.TextureField((object)((Sprite)value).texture, label, isPrivate);

		private void AnimatorControllerPlayableField(object value, string label, bool isPrivate)
		{
			UnityEngine.Animations.AnimatorControllerPlayable playable = (UnityEngine.Animations.AnimatorControllerPlayable)value;
			Field($"AnimatorControllerPlayable{{Handle: {playable.GetHandle()}}}", label, isPrivate);
			ArrayField(playable.GetLayers(), label + "Layers", isPrivate, label.Replace("[P]", "").Replace("[F]", "").Trim() + "Layers");
			ArrayField(playable.GetParameters(), label + "Parameters", isPrivate, label.Replace("[P]", "").Replace("[F]", "").Trim() + "Parameters");
		}

		private void AnimationPlayableOutputField(object value, string label, bool isPrivate)
		{
			UnityEngine.Animations.AnimationPlayableOutput output = (UnityEngine.Animations.AnimationPlayableOutput)value;
			Field($"AnimationPlayableOutput{{Target: {output.GetTarget()} | Handle: {output.GetHandle()}}}", label, isPrivate);
		}

		private void PlayableField(object value, string label, bool isPrivate)
		{
			UnityEngine.Playables.Playable playable = (UnityEngine.Playables.Playable)value;
			Field($"{{Type: {playable.GetPlayableType()} | Handle: {playable.GetHandle()}}}", label, isPrivate);
		}

		private void PlayableOutputField(object value, string label, bool isPrivate)
		{
			UnityEngine.Playables.PlayableOutput output = (UnityEngine.Playables.PlayableOutput)value;
			Field($"{{Type: {output.GetPlayableOutputType()} | Handle: {output.GetHandle()}}}", label, isPrivate);
		}

		private void PlayableGraphField(object value, string label, bool isPrivate)
		{
			UnityEngine.Playables.PlayableGraph graph = (UnityEngine.Playables.PlayableGraph)value;
			Field($"{{PlayableCount: {graph.GetPlayableCount()} | TimeUpdateMode: {graph.GetTimeUpdateMode()} | Resolver: {graph.GetResolver()}}}", label, isPrivate);
			ArrayField(graph.GetOutputs(), label + "Outputs", isPrivate, label.Replace("[P]","").Replace("[F]", "").Trim()+ "Outputs");
			ArrayField(graph.GetRootPlayables(), label + "RootPlayables", isPrivate, label.Replace("[P]","").Replace("[F]", "").Trim()+ "RootPlayables");
		}

		private void AnimatorControllerParameterField(object value, string label, bool isPrivate)
		{
			AnimatorControllerParameter controllerParameter = (AnimatorControllerParameter)value;
			Field($"{controllerParameter.name} {{Type: {controllerParameter.type} | Hash: {controllerParameter.nameHash} | Boolean: {controllerParameter.defaultBool} | Float: {controllerParameter.defaultFloat} | Integer: {controllerParameter.defaultInt}}}", label, isPrivate);
		}

		private void AnimationEventField(object value, string label, bool isPrivate)
		{
			AnimationEvent @event = (AnimationEvent)value;
			Field($"{{Function: {@event.functionName} | Object: {@event.objectReferenceParameter} | String: {@event.stringParameter} | Float: {@event.floatParameter} | Integer: {@event.intParameter}}}", label, isPrivate);
		}
		

		private void TranslationField(object value, string label, bool isPrivate)
		{
			Translation translation = (Translation)value;
			Field($"{translation.language}{{{translation.text}}}", label, isPrivate);
		}
		

		private void TranslationCollectionField(object value, string label, bool isPrivate)
		{
			TranslationCollection translations = (TranslationCollection)value;
			Field($"TranslationCollection{{{translations.GetEnglishText()},{translations.GetJapaneseText()}}}", label, isPrivate);
		}

		private void Vector4Field(object value, string label, bool isPrivate)
		{
			GUILayout.BeginHorizontal();

			DrawFieldLabel(label, isPrivate);

			Vector4 vector = (Vector4)value;
			DrawLabelledField(vector.x, "X");
			GUILayout.Space(5);
			DrawLabelledField(vector.y, "Y");
			GUILayout.Space(5);
			DrawLabelledField(vector.z, "Z");
			GUILayout.Space(5);
			DrawLabelledField(vector.w, "W");

			GUILayout.EndHorizontal();
		}

		private void Vector3Field(object value, string label, bool isPrivate)
		{
			GUILayout.BeginHorizontal();

			DrawFieldLabel(label, isPrivate);

			Vector3 vector = (Vector3)value;
			DrawLabelledField(vector.x, "X");
			GUILayout.Space(5);
			DrawLabelledField(vector.y, "Y");
			GUILayout.Space(5);
			DrawLabelledField(vector.z, "Z");

			GUILayout.EndHorizontal();
		}

		private void Vector2Field(object value, string label, bool isPrivate)
		{
			GUILayout.BeginHorizontal();

			DrawFieldLabel(label, isPrivate);

			Vector2 vector = (Vector2)value;
			DrawLabelledField(vector.x, "X");
			GUILayout.Space(5);
			DrawLabelledField(vector.y, "Y");

			GUILayout.EndHorizontal();
		}

		private void LayerField(int value, string label, bool isPrivate)
		{
			GUILayout.BeginHorizontal();

			DrawFieldLabel(label + " #" + value, isPrivate);

			GUILayout.Label(LayerMask.LayerToName(value), GUI.skin.textField);

			GUILayout.EndHorizontal();
		}

		private void StringField(object value, string label, bool isPrivate)
		{
			GUILayout.BeginHorizontal();

			DrawFieldLabel(label, isPrivate);

			GUILayout.Label(value.ToString(), GUI.skin.textField);

			GUILayout.EndHorizontal();
		}

		private void MeshField(object value, string label, bool isPrivate)
		{
			Mesh mesh = (Mesh)value;
			GUILayout.BeginHorizontal();

			DrawFieldLabel(label, isPrivate);

			if (mesh != null)
			{
				DrawLabelledField(mesh.name, "Name");
				GUILayout.Space(5);
				if (GUILayout.Button("Save", GUI.skin.textField))
				{
					MeshUtils.CacheMesh(mesh.name, mesh);
					MeshUtils.OpenFolder();
				}
			}
			else
				GUILayout.Label("null", GUI.skin.textField);

			GUILayout.EndHorizontal();
		}

		private void MusicTrackField(object value, string label, bool isPrivate)
		{
			MusicTrack musicTrack = (MusicTrack)value;
			GUILayout.BeginHorizontal();

			DrawFieldLabel(label, isPrivate);

			if (musicTrack != null)
			{
				DrawLabelledField(musicTrack.clip, "Clip");
				GUILayout.Space(5);
				DrawLabelledField(musicTrack.nextTrack, "Next Track");
			}
			else
				GUILayout.Label("null", GUI.skin.textField);

			GUILayout.EndHorizontal();
		}

		private void DrawFieldLabel(string label, bool isPrivate, bool useIndent = true)
		{
			GUI.contentColor = isPrivate ? Color.gray : Color.white;
			GUILayout.Space(4 + (useIndent ? lastIndent : 0));
			GUILayout.Label(label, GUILayout.ExpandWidth(false), GUILayout.Width(LABEL_WIDTH));
			GUI.contentColor = Color.white;
		}
		
		private void DrawLabelledField(object value, string label)
		{
			GUILayout.Label(label, GUILayout.ExpandWidth(false));
			GUILayout.Label(value.ToString(), GUI.skin.textField);
		}
	}
}