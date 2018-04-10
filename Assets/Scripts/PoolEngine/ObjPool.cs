using System.Collections.Generic;

namespace PoolEngine
{
    public interface IResetable
    {
         void ObjPoolDataReset();
    }


    public class ObjPool<T>where T:class, new()
    {
        private string poolname;
        private List<T> m_objectList;
        private int m_next;

        public ObjPool(int bufferSize)
        {
            m_objectList = new List<T>(bufferSize);
            m_next = 0;
        }

        public T New()
        {
            if(m_next<m_objectList.Count)
            {
                T t = m_objectList[m_next];
                m_next++;
                return t;
            }
            else
            {
                T t = new T();
                m_objectList.Add(t);
                m_next++;
                return t;
            }
        }

        public void ResetAll()
        {
            m_next = 0;
            //for(int i =0; i < m_objectList.Count;i++)
            //{
            //    m_objectList[i].ObjPoolDataReset();
            //}
        }
    }
}
