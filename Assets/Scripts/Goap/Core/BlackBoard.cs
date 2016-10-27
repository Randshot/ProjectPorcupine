using System.Collections.Generic;
using System.Diagnostics;
using UnityEngine;

public class BlackBoard {
    Dictionary<string,object> _data =new Dictionary<string, object>();

    public void AddData(string key, object val)
    {
        if (!_data.ContainsKey(key))
            _data.Add(key, val);
        else
            _data[key] = val;
    }

    public object GetData(string key)
    {
        if (_data.ContainsKey(key))
            return _data[key];
        return null;
    }
}
