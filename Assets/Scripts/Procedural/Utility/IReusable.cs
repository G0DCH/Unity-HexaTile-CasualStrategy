using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TilePuzzle.Procedural
{
    public interface IReusable
    {
        /// <summary>
        /// 오브젝트 풀에서 꺼내 재사용할때 실행
        /// </summary>
        void OnReuse();

        /// <summary>
        /// 삭제하는 대신 오브젝트 풀에 들어갈때 실행
        /// </summary>
        void OnPooling();
    }
}
