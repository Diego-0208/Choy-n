using System;
using UnityEngine;

[CreateAssetMenu(fileName = "Almanaque", menuName = "Scriptable Objects/Almanaque")]
public class Almanaque : ScriptableObject
{
    public Almanaque almanaque;
    public Sprite img;
    public string nombre;

    [TextArea(3,5)]
    public string desc;
    public string altura;

    [TextArea(3,5)]
    public string cuidados;

    public void LeerInformacion(string name, string descripcion, string alto, string cuidado, Sprite sprite)
    {
        nombre = name;
        desc = descripcion;
        altura = alto;
        cuidados = cuidado;
        img = sprite;
    }

}
