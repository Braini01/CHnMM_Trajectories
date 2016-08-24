using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace LfS.ModelLib.Common
{
    public interface IModelState
    {

    }

    public class Proxel
    {
        public IModelState State { get; private set; }
        public double P { get; set; }

        public Proxel(IModelState s, double p)
        {
            State = s;
            P = p;
        }
    }

    public class ProxelSet : ICollection<Proxel>
    {
        private Dictionary<IModelState, Proxel> proxels = new Dictionary<IModelState, Proxel>();

        public void Add(Proxel p)
        {
            //here the Proxelmerging happens
            var stateP = getProxelByState(p.State);
            if (stateP != null) 
                stateP.P += p.P; //ToDo: check if change to stateP is propagated to the dictionary
            else
            {
                proxels[p.State] = p;
                Count++;
            }
        }

        public int Count { get; private set; }

        public Proxel getProxelByState(IModelState s)
        {
            Proxel proxel;
            return (proxels.TryGetValue(s, out proxel)) ? proxel : null;
        }

        public IEnumerator<Proxel> GetEnumerator()
        {
            return proxels.Values.GetEnumerator();
        }

        System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator()
        {
            return proxels.Values.GetEnumerator();
        }

        public void Clear()
        {
            proxels.Clear();
        }

        public bool Contains(Proxel item)
        {
            return proxels.ContainsKey(item.State);
        }

        public void CopyTo(Proxel[] array, int arrayIndex)
        {
            proxels.Values.CopyTo(array, arrayIndex);
        }

        public bool IsReadOnly
        {
            get { return false; }
        }

        public bool Remove(Proxel item)
        {
            return proxels.Remove(item.State);
        }
    }
}