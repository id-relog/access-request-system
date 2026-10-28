using System;
using System.Threading;
using System.Threading.Tasks;

namespace AccessRequestSystem.BLL.Services
{

    /// <summary>
    /// Физическое предоставление/отзыв доступа к сайту.
    /// В продакшене — вызов API прокси/фаервола/AD.
    /// В демо-версии — локальный прокси на Titanium.Web.Proxy.
    /// </summary>
    public interface IProvisioningService
    {
        /// <summary>Разблокировать сайт для сотрудника.</summary>
        Task GrantAccessAsync(string domain, Guid employeeId, CancellationToken ct = default);

        /// <summary>Заблокировать сайт для сотрудника.</summary>
        Task RevokeAccessAsync(string domain, Guid employeeId, CancellationToken ct = default);

        /// <summary>Есть ли сейчас у сотрудника действующий доступ к домену — вызывается прокси-агентом на каждый запрос клиента.</summary>
        Task<bool> HasAccessAsync(string domain, Guid employeeId, CancellationToken ct = default);
    }
}