using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using SLua;

[CustomLuaClass]
public class XAvaterConfigure : XScriptAssetUpdateObject
{
    public string       skeleton;
    public string[]     elements;
}
