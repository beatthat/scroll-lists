using System.Collections;
using BeatThat.Bindings;
using BeatThat.ItemManagers;
using BeatThat.SafeRefs;
using BeatThat.TransformPathExt;
using UnityEngine;
using UnityEngine.Events;

namespace BeatThat.ScrollLists
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
                Debug.LogWarning("[" + animator.Path() + "] Missing required CenterOn child component. Maybe this behaviour is no longer wanted?");
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

            (this.controller as MonoBehaviour).StartCoroutine(WaitThenCenterOn(rt));
			
		}
		private UnityAction selectedItemUpdatedAction { get { return m_selectedItemUpdatedAction?? (m_selectedItemUpdatedAction = this.OnSelectedItemUpdated); } }
		private UnityAction m_selectedItemUpdatedAction;

		private CenterOn centerOn { get { return m_centerOn.value; } set { m_centerOn = new SafeRef<CenterOn>(value); } }
		private SafeRef<CenterOn> m_centerOn;

        private IEnumerator WaitThenCenterOn(RectTransform rt, int waitFrames = 2)
        {
            for (var i = 0; i < waitFrames; i++) {
                yield return new WaitForEndOfFrame();
            }

            this.centerOn.Center(rt);
        }
	}
}



