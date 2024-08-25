using Scriptables;

namespace Interfaces {
    public interface IEquipmentSlot {
        public ItemSO GetInfo();
        
        public void Pick();
        public void Assign(ItemSO item);
        public void Throw();
        public void Remove();
    }
}