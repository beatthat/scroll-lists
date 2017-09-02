using UnityEngine;
using UnityEngine.UI;

namespace BeatThat.UI
{
	/// <summary>
	/// Adjust scroll position of a scrolling list so that (some point between) 
	/// the center of two items aligns with the center of the viewport.
	/// 
	/// This can be better for smooth scrolling along with a playback function
	/// because it makes it easier to accomadate for both uneven 'time' values of each row 
	/// as well as variable margin spaces between items.
	/// </summary>
	public class CenterOn : MonoBehaviour
	{
		[Tooltip("Avg time is should take to reach scroll target")]
		public float m_smoothTime = 0.1f;

		[Tooltip("When smooth scrolling, and distance to target falls below this threshold, clamp")]
		public float m_clampThreshold = 0.001f; 
		public ScrollRect m_scrollRect;

		public bool m_debug;

		/// <summary>
		/// Adjust scroll position of a scrolling list so that the given item is at the center of the viewport.
		/// 
		/// <param name="item">The RectTransform of the item to center on</param>
		/// <param name="snap">If TRUE, the snaps to target position, if FALSE may smooth damp to the target positon of several frames. Default is snap=FALSE</param>
		public void Center(Transform item, bool snap = false)
		{
			if(item == null) {
				if(m_debug) {
					Debug.LogWarning("[" + Time.frameCount + "][" + this.Path() + "] " + GetType() + " Center called with null");
				}
				return;
			}

			var itemRT = item as RectTransform;
			if(itemRT == null) {
				Debug.LogWarning("CenterOn works only on RectTransform. Call on invalid " + item.Path());
				return;
			}

			var scRect = this.scrollRect;

			var contentRect = scRect.content.rect;
			var vpRectInContentSpace = scRect.content.InverseTransformRect(scRect.GetViewport());

			// what distance in x and y is the scroll rect capable of scrolling...?
			var totalScrollDist = new Vector2(
				scRect.horizontal? contentRect.width - vpRectInContentSpace.width: 0f,
				scRect.vertical ? contentRect.height - vpRectInContentSpace.height: 0f
			);

			// if the scroll rect can't scroll at all (content is smaller than the viewport), bail out
			if(totalScrollDist.x <= 0f && totalScrollDist.y <= 0f) { 
				if(m_debug) {
					Debug.LogWarning("[" + Time.frameCount + "][" + this.Path() + "] " + GetType() + " total scroll dist is non positive");
				}

				return;
			}
				
			// what is the scroll position (in content-rect space) to center the item
			var tgtScrollContentSpace = CalcScrollToCenterOf(vpRectInContentSpace, itemRT);
			var curScroll = scRect.normalizedPosition;

			// convert the above to normalized values
			m_scrollTgt = new Vector2(
				totalScrollDist.x > 0f? Mathf.Clamp01(tgtScrollContentSpace.x / totalScrollDist.x): curScroll.x, 
				totalScrollDist.y > 0f? Mathf.Clamp01(tgtScrollContentSpace.y / totalScrollDist.y): curScroll.y
			);

			if(m_debug) {
				Debug.LogWarning("[" + Time.frameCount + "][" + this.Path() + "] " + GetType() + " set scroll target to " + m_scrollTgt);
			}

			if(snap) {
				this.scrollRect.normalizedPosition = m_scrollTgt;
				m_scrollVelocity = Vector3.zero;
				return;
			}
				
			this.enabled = true;
		}


		private Vector2 CalcScrollToCenterOf(Rect vpRectInContentSpace, RectTransform item)
		{
			var scRect = this.scrollRect;
			var contentRect = scRect.content.rect;
			var itemRectInContentSpace = scRect.content.InverseTransformRect(item);

			var itemPos = itemRectInContentSpace.center - contentRect.min; // TODO: not sure this is right to always use bottom-left corner?
			return itemPos - new Vector2((vpRectInContentSpace.width / 2f), (vpRectInContentSpace.height / 2f));
		}

		void Awake()
		{
			if(m_scrollRect == null) { m_scrollRect = GetComponent<ScrollRect>(); }
			this.enabled = false;
		}

		void LateUpdate()
		{
			var scRect = this.scrollRect;

			var curPos = scRect.normalizedPosition;
			var newPos = Vector2.SmoothDamp(curPos, m_scrollTgt, ref m_scrollVelocity, m_smoothTime, float.MaxValue, Time.unscaledDeltaTime);

			if(!scRect.horizontal) {
				newPos.x = curPos.x;
			}

			if(!scRect.vertical) {
				newPos.y = curPos.y;
			}

			var dist = Vector3.Distance (curPos, newPos);
			if(dist > m_clampThreshold) {
				this.scrollRect.normalizedPosition = newPos;
				return;
			}

			newPos = m_scrollTgt;
			m_scrollVelocity = Vector2.zero;
			this.scrollRect.normalizedPosition = newPos;
			this.enabled = false; 
		}
			
		private ScrollRect scrollRect { get { return m_scrollRect?? (m_scrollRect = GetComponent<ScrollRect>()); } }
		private Vector2 m_scrollVelocity = Vector3.zero;
		private Vector2 m_scrollTgt = Vector2.zero;
	}
}

