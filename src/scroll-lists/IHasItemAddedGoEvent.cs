using UnityEngine.Events;
using UnityEngine;


namespace BeatThat
{
	public interface IHasItemAddedGoEvent 
	{
		UnityEvent<GameObject> itemAddedGO { get; }
	}
}