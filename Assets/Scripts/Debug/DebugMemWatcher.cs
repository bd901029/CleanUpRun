using UnityEngine;

public class DebugMemWatcher : MonoBehaviour 
{
	uint m_HeapSize;
	uint m_HeapMem;
	
	uint m_TextureMem;
	
#if ENABLE_PROFILER
	void Start()
	{
		UpdateMemoryCounts();
	}
	
	void Update () 
	{
		DebugDisplay.Print("MonoHeap", string.Format( "{0}/{0}", 
													  FormatMemory(m_HeapMem), 
													  FormatMemory(m_HeapSize)) );
		DebugDisplay.Print("TextureMem", FormatMemory(m_TextureMem));
	}
	
	void UpdateMemoryCounts()
	{
		m_HeapMem = Profiler.GetMonoUsedSize();
		m_HeapSize = Profiler.GetMonoHeapSize();
		
		m_TextureMem = GetMemory(typeof(Texture));
		
		Invoke("UpdateMemoryCounts", 0.1f);
	}
#endif
	
	uint GetMemory(System.Type type)
	{
		uint size = 0;
		Object[] objs = Resources.FindObjectsOfTypeAll(type);
		foreach(Object o in objs)
			size += (uint)Profiler.GetRuntimeMemorySize(o);
		return size;
	}
	
	static string[] s_MemSuffixes =
	{
		"B",
		"KB",
		"MB",
		"GB"
	};
	
	public static string FormatMemory(uint memInBytes)
	{
		float bytes = (float)memInBytes;
		int i = 0;
		while(bytes > 1024.0f * 1.1f)
		{
			bytes /= 1024.0f;
			++i;
		}
		return string.Format("{0:0.###}{1}", bytes, s_MemSuffixes[i]);
	}
}
