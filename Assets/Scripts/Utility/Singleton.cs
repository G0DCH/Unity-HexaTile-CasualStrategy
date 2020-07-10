using UnityEngine;

namespace TilePuzzle.Utility
{
    public class Singleton<T> : MonoBehaviour where T : MonoBehaviour
    {
        private static T instance;

        public static T Instance
        {
            get
            {
                //인스턴스가 없다면
                if (instance == null)
                {
                    //찾아서 할당해라
                    instance = FindObjectOfType(typeof(T)) as T;
                    //찾아도 존재하지 않는다면
                    if (instance == null)
                    {
                        //에러 메시지
                        Debug.LogError("There's no active " + typeof(T) + " in this scene");
                    }
                }

                return instance;
            }
        }
    }
}