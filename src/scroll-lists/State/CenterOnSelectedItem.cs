using UnityEngine;
using UnityEngine.Events;
using BeatThat.UI;

namespace BeatThat
{
	public class CenterOnSelectedItem : BindingStateBehaviour<IHasSelectedItem>
	{
		public bool m_debug;

		override protected bool WillEnter(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
		{
			if(this.centerOn != null) {
				return true;
			}

			this.centerOn = animator.GetComponentInChildren<CenterOn>(true);

			if(this.centerOn == null) {
				Debug.LogWarning("Missing required CenterOn child component. Maybe this behaviour is no longer wanted?");
				return false;
			}

			return true;
		}

		override protected void DidEnter()
		{
			Bind(this.controller.selectedItemUpdated, this.selectedItemUpdatedAction);
			OnSelectedItemUpdated ();
		}

		private void OnSelectedItemUpdated()
		{
			var go = this.controller.selectedGameObject;
			if(go == null) {
				#if UNITY_EDITOR || DEBUG_UNSTRIP
				if(m_debug) {
					Debug.Log("[" + Time.frameCount + "] " + GetType() + "::OnSelectedItemUpdated selected item is NULL");
				}
				#endif
				return;
			}
			var rt = go.transform as RectTransform;
			if(rt == null) {
				#if UNITY_EDITOR || DEBUG_UNSTRIP
				Debug.LogWarning("Expect selected items to have a rect transform");
				#endif
				return;
			}

			#if UNITY_EDITOR || DEBUG_UNSTRIP
			if(m_debug) {
				Debug.Log("[" + Time.frameCount + "] " + GetType() + "::OnSelectedItemUpdated will center on " + go.name);
			}
			#endif

			this.centerOn.Center(rt);
		}
		private UnityAction selectedItemUpdatedAction { get { return m_selectedItemUpdatedAction?? (m_selectedItemUpdatedAction = this.OnSelectedItemUpdated); } }
		private UnityAction m_selectedItemUpdatedAction;

		private CenterOn centerOn { get { return m_centerOn.value; } set { m_centerOn = new SafeRef<CenterOn>(value); } }
		private SafeRef<CenterOn> m_centerOn;
	}
}
