using System.Diagnostics;
using System.Runtime.Serialization;
using GameServer.Domain.Object.Components;
using Newtonsoft.Json;

namespace GameServer.Domain.Object
{
    public class ObjectEntity
    {
        public string Id { get; }

        private Dictionary<Type, GameComponent> components = new();
        public int HasConponentNum => components.Count;
        public List<GameComponent> AllComponents => components.Values.ToList();

        public ObjectEntity(string id)
        {
            Id = id;
        }

        public void Add(GameComponent component)
        {
            components[component.GetType()] = component;
        }

        public bool HasComponent<T>() where T : GameComponent
        {
            return components.ContainsKey(typeof(T));
        }

        public bool TryGetComponent<T>(out T component) where T : GameComponent
        {
            if (components.TryGetValue(typeof(T), out var comp))
            {
                component = (T)comp;
                return true;
            }
            component = null;
            return false;
        }

        public T GetComponent<T>() where T : GameComponent
        {
            if (!HasComponent<T>()) return null;

            return (T)components[typeof(T)];
        }

        public ObjectEntity Clone()
        {
            var copy = new ObjectEntity(this.Id);
            foreach (var kvp in this.components)
            {
                // Še GameComponent ‚ð Clone ‚µ‚Ä’Ç‰Á
                copy.components[kvp.Key] = kvp.Value.Clone();
                Console.WriteLine(copy.components[kvp.Key]);
            }
            return copy;
        }
    }
}