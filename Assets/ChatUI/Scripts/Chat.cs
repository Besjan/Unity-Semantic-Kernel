using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class Chat : MonoBehaviour
{
	public UnityEvent<string> OnMessageSent;

	[HideInInspector] public string Owner;
	public List<string> Members = new List<string>();
	public MessageContainer Container;

	public string RandomMember => Members[Random.Range(0, Members.Count)];

	public void ReceiveMessage(Message message)
	{
		Container.AddMessage(message);
		OnMessageSent.Invoke(message.Content);
	}

	private void Reset() =>
	  Container = FindObjectOfType<MessageContainer>();
}