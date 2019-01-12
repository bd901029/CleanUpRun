using UnityEngine;

using pumpkin.display;

public class LoadingScreen : MovieClipOverlayCameraBehaviour 
{
	static string LevelName { get; set; }
	[SerializeField]
	string InitialLevel;
	
	public static void LoadLevel(string levelName)
	{
		LevelName = levelName;
		Application.LoadLevel("01-LoadingScreen");
	}
	
	AsyncOperation m_Loader;

	void Start () 
	{
		MovieClip mc = new MovieClip("UI/Loading.swf:LoadingScreen");
		
		Vector2 screen = new Vector2(Screen.width, Screen.height);
		// central section is 960x480. We want at least that much on the screen
		float sx = screen.x / 640.0f;
		float sy = screen.y / 960.0f;
		float scale = 1.0f;
		if(sx <= 1.0f || sy <= 1.0f)
			scale = Mathf.Max(sx, sy);
		else
		{
			sx = screen.x / mc.width;
			sy = screen.y / mc.height;
			scale = Mathf.Max(sx, sy);
		}
		// now work out how much to offset to get the center in the center
		Vector2 size = screen / scale;
		size /= 2.0f;
		Vector2 delta = size - (new Vector2(mc.width, mc.height) / 2.0f);
		delta *= scale;
		mc.x = delta.x;
		mc.y = delta.y;
		mc.scaleX = scale;
		mc.scaleY = scale;
		
		stage.addChild(mc);
	
		string level = "";
		if(LevelName == null || LevelName.Length == 0)
		{
			level = InitialLevel;
			MusicManager.Play("Main");
		}
		else
			level = LevelName;
		m_Loader = Application.LoadLevelAdditiveAsync(level);
	}
	
	// Update is called once per frame
	void Update () 
	{
		if(m_Loader == null || m_Loader.isDone)
			Destroy(gameObject);
		
		base.Update();
	}
}
