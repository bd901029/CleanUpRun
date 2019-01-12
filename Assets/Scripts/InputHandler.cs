using UnityEngine;

public class InputHandler : MonoBehaviour 
{
	public enum Direction
	{
		Left,
		Right,
		Up,
		Down
	}
	
	[System.Serializable]
	class MouseState
	{
	
		float SwipeMinDistance = 10.0f; // percent of the screen width to be regarded as a swipe
		float SwipeMaxTime = 5.0f; // How long (s) a swipe is allowed to be
		
		bool swiped;
		
		public Vector2 Position;
		
		public void Update()
		{
			Swipe = false;
			if(Input.anyKeyDown)
			{
				Vector3 pos = Input.mousePosition;
				Position = new Vector2(pos.x, pos.y);
				swiped = false;
			}
			else if(!swiped && Input.anyKey)
			{
				float minSwipe = (Screen.width * SwipeMinDistance) / 100.0f;
				Vector3 pos = Input.mousePosition;
				SwipeDir = new Vector2(pos.x, pos.y) - Position;
				if(SwipeDir.sqrMagnitude > minSwipe * minSwipe)
				{
					Swipe = true;
					swiped = true;
				}
			}
			else if(!Input.anyKey)
			{
				swiped = false;
				Swipe = false;
			}
				
		}
		
		public bool Swipe { get; private set; }
		public Vector2 SwipeDir { get; private set; }
	}
	MouseState m_Mouse = new MouseState();
	
	void Update () 
	{
		m_Mouse.Update();
		
		if(m_Mouse.Swipe)
			RegisterSwipe(m_Mouse.SwipeDir);
		/*
		foreach(Touch touch in Input.touches)
		{
			Debug.Log(touch.deltaTime);
			if( touch.phase == TouchPhase.Ended && 
				touch.deltaTime < SwipeMaxTime )
			{
				if(touch.deltaPosition.sqrMagnitude >= (minSwipe*minSwipe))
				{
					// we have a swipe!
					RegisterSwipe(touch.deltaPosition);
				}
			}
		}
		*/
		if(Input.GetKeyDown(KeyCode.LeftArrow))
			SwipeEvents(Direction.Left);
		if(Input.GetKeyDown(KeyCode.RightArrow))
			SwipeEvents(Direction.Right);
		if(Input.GetKeyDown(KeyCode.UpArrow))
			SwipeEvents(Direction.Up);
	}
	
	public delegate void SwipeDelegate(Direction dir);
	public SwipeDelegate SwipeEvents = new SwipeDelegate(_emptyDelegate);
	
	static void _emptyDelegate(Direction dir){}
	
	void RegisterSwipe(Vector2 dir)
	{
		if(Mathf.Abs(dir.y) > Mathf.Abs(dir.x))
		{
			// vertical swipe
			if(dir.y > 0)
				SwipeEvents(Direction.Up);
			else
				SwipeEvents(Direction.Down);
		}
		else
		{
			// horizontal swipe
			if(dir.x > 0)
				SwipeEvents(Direction.Right);
			else
				SwipeEvents(Direction.Left);
		}
	}
}
