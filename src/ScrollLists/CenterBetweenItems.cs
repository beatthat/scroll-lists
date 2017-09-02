using UnityEngine;
using UnityEngine.UI;

namespace BeatThat.Edit.Mobile
{
	/// <summary>
	/// Adjust scroll position of a scrolling list so that (some point between) 
	/// the center of two items aligns with the center of the viewport.
	/// 
	/// This can be better for smooth scrolling along with a playback function
	/// because it makes it easier to accomadate for both uneven 'time' values of each row 
	/// as well as variable margin spaces between items.
	/// </summary>
	public class CenterBetweenItems : MonoBehaviour
	{
		[Tooltip("Avg time is should take to reach scroll target")]
		public float m_smoothTime = 0.1f;
		public ScrollRect m_scrollRect;

		/// <summary>
		/// Adjust scroll position of a scrolling list so that (some point between) 
		/// the center of two items aligns with the center of the viewport.
		/// 
		/// This can be better for smooth scrolling along with a playback function
		/// because it makes it easier to accomadate for both uneven 'time' values of each row 
		/// as well as variable margin spaces between items.
		/// </summary>
		/// 
		/// <param name="itemA">The RectTransform of the first item</param>
		/// <param name="itemB">The RectTransform of the first item/param>
		/// <param name="interpolate">UNCLAMPED interpolation value where aligns of the center of itemA
		///  to the center of the viewport and 1 aligns the center of itemB to the center of the viewport</param>
		/// <param name="snap">If TRUE, the snaps to target position, if FALSE may smooth damp to the target positon of several frames. Default is snap=FALSE</param>
		public void CenterBetween(RectTransform itemA, RectTransform itemB, float interpolate, bool snap = false)
		{
//			Debug.LogError("[" + Time.frameCount + "] [" + this.Path() + "] " + GetType() + "::CenterBetween");


			var viewport = this.scrollRect.GetViewport();

			var contentRect = this.scrollRect.content.rect;
			var viewportRectInContentSpace = this.scrollRect.content.InverseTransformRect(viewport);

			var totalScrollDist = contentRect.height - viewportRectInContentSpace.height;
			if(totalScrollDist < 0f) { 
				return;
			}

			var itemA_scroll = CalcScrollToCenterOf(viewportRectInContentSpace, itemA);
			var itemB_scroll = CalcScrollToCenterOf(viewportRectInContentSpace, itemB);

			var tgt_scroll = Mathf.LerpUnclamped(itemA_scroll, itemB_scroll, interpolate);

			var tgt_normalized = Mathf.Clamp01(tgt_scroll / totalScrollDist);

			m_scrollTgt = new Vector2(0f, tgt_normalized);

			if(snap) {
				this.scrollRect.normalizedPosition = m_scrollTgt;
				m_scrollVelocity = Vector3.zero;
				return;
			}
				
			this.enabled = true;
		}


		private float CalcScrollToCenterOf(Rect viewportRectInContentSpace, RectTransform item)
		{
			var itemRect = m_scrollRect.content.InverseTransformRect(item);
			var itemPos = itemRect.center.y - m_scrollRect.content.rect.yMin;
			return itemPos - (viewportRectInContentSpace.height / 2f);
		}

		void Awake()
		{
			if(m_scrollRect == null) { m_scrollRect = GetComponent<ScrollRect>(); }
			this.enabled = false;
		}

		void LateUpdate()
		{
			var curPos = m_scrollRect.normalizedPosition;
			var dampedPos = Vector2.SmoothDamp(curPos, m_scrollTgt, ref m_scrollVelocity, m_smoothTime, float.MaxValue, Time.unscaledDeltaTime);

			if(Vector2.Distance (curPos, dampedPos) < 0.0001f) {
				this.scrollRect.normalizedPosition = m_scrollTgt;
				m_scrollVelocity = Vector2.zero;
				this.enabled = false;
				return;
			}

			this.scrollRect.normalizedPosition = dampedPos;
		}
			
		private ScrollRect scrollRect { get { return m_scrollRect?? (m_scrollRect = GetComponent<ScrollRect>()); } }
		private Vector2 m_scrollVelocity = Vector3.zero;
		private Vector2 m_scrollTgt = Vector2.zero;
	}
}

