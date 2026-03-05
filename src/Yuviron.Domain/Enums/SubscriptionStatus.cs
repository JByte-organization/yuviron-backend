using System;
using System.Collections.Generic;
using System.Text;

namespace Yuviron.Domain.Enums;

public enum SubscriptionStatus
{
    /// Ожидает оплаты (например, юзер перешел на страницу оплаты, но еще не ввел карту)
    Pending = 0,

    /// Подписка активна, деньги списываются, доступ есть
    Active = 1,

    /// Подписка отменена юзером (или системой при удалении), автопродления не будет
    Cancelled = 2,

    /// Подписка истекла (время EndAt прошло)
    Expired = 3,

    /// Ошибка списания средств (недостаточно денег на карте)
    PastDue = 4
}
