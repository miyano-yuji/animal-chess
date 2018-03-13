// GetComponentsInChildrenで自分自身を含まないようにする拡張メソッド
// http://baba-s.hatenablog.com/entry/2014/06/05/220224

using System.Linq;
using UnityEngine;

public static class ExtensionMethods
{
	public static T[] GetComponentsInChildrenWithoutSelf<T>(this GameObject self) where T : Component
	{
		return self.GetComponentsInChildren<T>().Where(c => self != c.gameObject).ToArray();
	}
}