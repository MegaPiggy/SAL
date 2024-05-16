namespace UnityEngine
{
	public class AnimatorControllerLayer
	{
		private int index;
		private string name;
		private float weight;
        private AnimatorClipInfo[] currentAnimatorClipInfo;
        private AnimatorStateInfo currentAnimatorStateInfo;
        private AnimatorClipInfo[] nextAnimatorClipInfo;
        private AnimatorStateInfo nextAnimatorStateInfo;

        public int Index => index;
		public string Name => name;
		public float Weight => weight;

		internal AnimatorControllerLayer(int index, string name, float weight, AnimatorClipInfo[] currentAnimatorClipInfo, AnimatorStateInfo currentAnimatorStateInfo, AnimatorClipInfo[] nextAnimatorClipInfo, AnimatorStateInfo nextAnimatorStateInfo)
		{
			this.index = index;
			this.name = name;
			this.weight = weight;
            this.currentAnimatorClipInfo = currentAnimatorClipInfo;
            this.currentAnimatorStateInfo = currentAnimatorStateInfo;
            this.nextAnimatorClipInfo = nextAnimatorClipInfo;
            this.nextAnimatorStateInfo = nextAnimatorStateInfo;
        }

        public override string ToString()
        {
            return $"{{Index: {index} | Name: {name} | Weight: {weight} | CurrentAnimatorClipInfoCount: {currentAnimatorClipInfo.Length} | CurrentAnimatorStateInfo: {currentAnimatorStateInfo.fullPathHash} | NextAnimatorClipInfoCount: {nextAnimatorClipInfo.Length} | NextAnimatorStateInfo: {nextAnimatorStateInfo.fullPathHash}}}";
        }
    }
}