using UnityEngine;
using UnityEngine.Events;
using BeatThat.UI;

namespace BeatThat
{
	public class CenterOnSelectedItem : BindingStateBehaviour<IHasSelectedItem>
	{
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

		override protected void BindState()
		{
			Bind(this.controller.selectedItemUpdated, this.selectedItemUpdatedAction);
		}

		private void OnSelectedItemUpdated()
		{
			var go = this.controller.selectedGameObject;
			if(go == null) {
				return;
			}
			var rt = go.transform as RectTransform;
			if(rt == null) {
				Debug.LogWarning("Expect selected items to have a rect transform");
				return;
			}

			this.centerOn.Center(rt);
		}
		private UnityAction selectedItemUpdatedAction { get { return m_selectedItemUpdatedAction?? (m_selectedItemUpdatedAction = this.OnSelectedItemUpdated); } }
		private UnityAction m_selectedItemUpdatedAction;

		private CenterOn centerOn { get { return m_centerOn.value; } set { m_centerOn = new SafeRef<CenterOn>(value); } }
		private SafeRef<CenterOn> m_centerOn;
	}
}
