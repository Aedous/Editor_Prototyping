using UnityEngine;
using System.Collections;

public static class Global{
	
	public enum ToolSet
	{
		BRUSH = 0,
		ERASE,
		FILL,
        EMPTY,
        PULL, //Pull a certain amount of blocks with a certain type of depth
        PUSH, //Opposite of pull
        DUPLICATE
	}
	
}
