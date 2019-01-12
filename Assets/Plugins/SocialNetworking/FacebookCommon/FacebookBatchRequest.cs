using UnityEngine;
using System.Text;
using System.Collections;
using System.Collections.Generic;


public class FacebookBatchRequest
{
	public Dictionary<string,string> _parameters = new Dictionary<string,string>();
	private Dictionary<string,object> _requestDict = new Dictionary<string,object>();
	
	
	public FacebookBatchRequest( string relativeUrl, string method )
	{
		_requestDict["method"] = method.ToUpper();
		_requestDict["relative_url"] = relativeUrl;
	}
	
	
	public void addParameter( string key, string value )
	{
		_parameters[key] = value;
	}
	
	
	public Dictionary<string,object> requestDictionary()
	{
		if( _parameters.Count > 0 )
		{
			var builder = new StringBuilder();
			foreach( var p in _parameters )
				builder.AppendFormat( "{0}={1}&", p.Key, p.Value );
			builder.Remove( builder.Length - 1, 1 );
			
			_requestDict["body"] = builder.ToString();
		}
		
		return _requestDict;
	}
}
