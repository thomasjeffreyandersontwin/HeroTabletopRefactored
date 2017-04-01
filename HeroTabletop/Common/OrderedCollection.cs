using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HeroVirtualTableTop.Common
{
    public interface OrderedCollection<T> : IDictionary<string, T> where T : OrderedElement
    {
        void InsertElement(T element);
        T this[int index] { get; set; }
        
        void InsertElementAfter(T item, T precedingItem);
        void RemoveElement(T item);
        void InsertMany(List<T> elements);
        IEnumerator<T> ByOrder { get; }
    }



    public interface OrderedElement
    {
        string Name { get; set; }
        int Order { get; set; }
        

    }

    public class OrderedCollectionImpl<T> : Dictionary<string, T>, OrderedCollection<T> where T : OrderedElement
    {
        protected Dictionary<int, T> ListByOrder => (from item in Values orderby item.Order select item).ToDictionary(x => x.Order); 
        public T this[int order]
        {
            get { return ListByOrder[order]; }
            set { ListByOrder[order] = value; }
        }
        public void InsertMany(List<T> list)
        {
            foreach (var item in list)
                InsertElement(item);
        }
        public virtual void InsertElement(T item)
        {
            var count = 0;
            if (ListByOrder != null && ListByOrder.Count > 0)
                count = ListByOrder.Last().Key;
            item.Order = count + 1;
            Add(item.Name, item);
        }
        public void InsertElementAfter(T elementToInsert, T precedingElement)
        {
            if (ContainsValue(elementToInsert))
            {
                RemoveElement(elementToInsert);
            }
            var precedingOrder = precedingElement.Order;
            foreach (var item in ListByOrder.Reverse())
                if (item.Key > precedingOrder)
                {
                    item.Value.Order++;
                    ListByOrder[item.Value.Order] = item.Value;
                }
            elementToInsert.Order = precedingOrder + 1;
            ListByOrder[elementToInsert.Order] = elementToInsert;
            Add(elementToInsert.Name, elementToInsert);
        }
        public void RemoveElement(T elementToRemove)
        {
            var deletedOrder = elementToRemove.Order;
            Remove(elementToRemove.Name);
            ListByOrder.Values.ToList().Where(c => c.Order > deletedOrder).ToList().ForEach(c => c.Order--);
        }
        public IEnumerator<T> ByOrder => ListByOrder.Values.GetEnumerator();


    }
}
