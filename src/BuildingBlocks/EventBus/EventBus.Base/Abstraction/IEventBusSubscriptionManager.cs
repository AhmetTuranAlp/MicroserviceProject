using EventBus.Base.Events;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EventBus.Base.Abstraction
{
    public interface IEventBusSubscriptionManager
    {
        event EventHandler<string> OnEventRemoved;

        /// <summary>
        /// Subscription olup olmadıgı kontrol ediliyor. Herhangi bir event dinleniyormu kontrol etmektedir.
        /// </summary>
        bool IsEmpty { get; }

        /// <summary>
        /// Subscription Ekleme
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <typeparam name="TH"></typeparam>
        void AddSubscription<T, TH>() where T : IntegrationEvent where TH : IIntegrationEventHandler<T>;

        /// <summary>
        /// Subscription Silme
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <typeparam name="TH"></typeparam>
        void RemoveSubscription<T, TH>() where TH : IIntegrationEventHandler<T> where T : IntegrationEvent;

        /// <summary>
        /// Dışarıdan gelen event type göre Subscription dinleme yapılıp yapılmadıgı kontrol edilecek.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        bool HasSubscriptionsForEvent<T>() where T : IntegrationEvent;

        /// <summary>
        /// Dışarıdan gelen eventName göre Subscription dinleme yapılıp yapılmadıgı kontrol edilecek.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        bool HasSubscriptionsForEvent(string eventName);

        /// <summary>
        /// eventName göre event'in tipine ve kendisine erişilmesi saglanmaktadır.
        /// </summary>
        /// <param name="eventName"></param>
        /// <returns></returns>
        Type GetEventTypeByName(string eventName);

        /// <summary>
        /// Bütün Subscription silinmektedir.
        /// </summary>
        void Clear();

        /// <summary>
        /// Gelen event type göre bütün Subscription ve Handler'ları dönmektedir.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        IEnumerable<SubscriptionInfo> GetHandlersForEvent<T>() where T : IntegrationEvent;

        /// <summary>
        /// Gelen eventName göre bütün Subscription ve Handler'ları dönmektedir.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="eventName"></param>
        /// <returns></returns>
        IEnumerable<SubscriptionInfo> GetHandlersForEvent(string eventName);

        /// <summary>
        /// Event'ın key bilgisi dönmektedir.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        string GetEventKey<T>();
    }
}
