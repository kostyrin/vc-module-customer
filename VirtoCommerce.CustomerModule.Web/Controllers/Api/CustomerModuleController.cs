﻿using System.Linq;
using System.Net;
using System.Web.Http;
using System.Web.Http.Description;
using VirtoCommerce.CustomerModule.Web.Model;
using VirtoCommerce.CustomerModule.Web.Security;
using VirtoCommerce.Domain.Commerce.Model.Search;
using VirtoCommerce.Domain.Customer.Model;
using VirtoCommerce.Domain.Customer.Services;
using VirtoCommerce.Platform.Core.Web.Security;

namespace VirtoCommerce.CustomerModule.Web.Controllers.Api
{
    [RoutePrefix("api")]
    [CheckPermission(Permission = CustomerPredefinedPermissions.Read)]
    public class CustomerModuleController : ApiController
    {
        private readonly IMemberService _memberService;
        private readonly IMemberSearchService _memberSearchService;

        public CustomerModuleController(IMemberService memberService, IMemberSearchService memberSearchService)
        {
            _memberService = memberService;
            _memberSearchService = memberSearchService;
        }

        /// <summary>
        /// Get organizations
        /// </summary>
        /// <remarks>Get array of all organizations.</remarks>
        [HttpGet]
        [Route("members/organizations")]
        [ResponseType(typeof(Organization[]))]
        public IHttpActionResult ListOrganizations()
        {
            var searchCriteria = new MembersSearchCriteria
            {
                MemberType = typeof(Organization).Name,
                DeepSearch = true,
                Take = int.MaxValue
            };
            var result = _memberSearchService.SearchMembers(searchCriteria);

            return Ok(result.Results.OfType<Organization>());
        }

        /// <summary>
        /// Get members
        /// </summary>
        /// <remarks>Get array of members satisfied search criteria.</remarks>
        /// <param name="criteria">concrete instance of SearchCriteria type type will be created by using PolymorphicMemberSearchCriteriaJsonConverter</param>
        [HttpPost]
        [Route("members/search")]
        [ResponseType(typeof(GenericSearchResult<Member>))]
        public IHttpActionResult Search(MembersSearchCriteria criteria)
        {
            var result = _memberSearchService.SearchMembers(criteria);
            return Ok(result);
        }

        /// <summary>
        /// Get member
        /// </summary>
        /// <param name="id">member id</param>
        [HttpGet]
        [Route("members/{id}")]
        [ResponseType(typeof(Member))]
        public IHttpActionResult GetMemberById(string id)
        {
            var retVal = _memberService.GetByIds(new[] { id }).FirstOrDefault();
            if (retVal != null)
            {
                // Casting to dynamic fixes a serialization error in XML formatter when the returned object type is derived from the Member class.
                return Ok((dynamic)retVal);
            }
            return Ok();
        }

        [HttpGet]
        [Route("members")]
        [ResponseType(typeof(Member))]
        public IHttpActionResult GetMembersByIds([FromUri] string[] ids)
        {
            var retVal = _memberService.GetByIds(ids);
            if (retVal != null)
            {
                // Casting to dynamic fixes a serialization error in XML formatter when the returned object type is derived from the Member class.
                return Ok(retVal.Cast<dynamic>().ToArray());
            }
            return Ok();
        }

        /// <summary>
        /// Create new member (can be any object inherited from Member type)
        /// </summary>
        /// <param name="member">concrete instance of abstract member type will be created by using PolymorphicMemberJsonConverter</param>
        /// <returns></returns>
        [HttpPost]
        [Route("members")]
        [ResponseType(typeof(Member))]
        [CheckPermission(Permission = CustomerPredefinedPermissions.Create)]
        public IHttpActionResult CreateMember([FromBody] Member member)
        {
            _memberService.SaveChanges(new[] { member });
            var retVal = _memberService.GetByIds(new[] { member.Id }).FirstOrDefault();

            // Casting to dynamic fixes a serialization error in XML formatter when the returned object type is derived from the Member class.
            return Ok((dynamic)retVal);
        }

        /// <summary>
        /// Update member
        /// </summary>
        /// <param name="member">concrete instance of abstract member type will be created by using PolymorphicMemberJsonConverter</param>
        [HttpPut]
        [Route("members")]
        [ResponseType(typeof(void))]
        [CheckPermission(Permission = CustomerPredefinedPermissions.Update)]
        public IHttpActionResult UpdateMember(Member member)
        {
            _memberService.SaveChanges(new[] { member });
            return StatusCode(HttpStatusCode.NoContent);
        }

        /// <summary>
        /// Delete members
        /// </summary>
        /// <remarks>Delete members by given array of ids.</remarks>
        /// <param name="ids">An array of members ids</param>
        [HttpDelete]
        [Route("members")]
        [ResponseType(typeof(void))]
        [CheckPermission(Permission = CustomerPredefinedPermissions.Delete)]
        public IHttpActionResult DeleteMembers([FromUri] string[] ids)
        {
            _memberService.Delete(ids);
            return StatusCode(HttpStatusCode.NoContent);
        }

        #region Special members for storefront C# API client  (because it not support polymorph types)

        /// <summary>
        /// Create contact
        /// </summary>
        [HttpPost]
        [Route("contacts")]
        [ResponseType(typeof(Contact))]
        [CheckPermission(Permission = CustomerPredefinedPermissions.Create)]
        public IHttpActionResult CreateContact(Contact contact)
        {
            return CreateMember(contact);
        }

        /// <summary>
        /// Update contact
        /// </summary>
        [HttpPut]
        [Route("contacts")]
        [ResponseType(typeof(void))]
        [CheckPermission(Permission = CustomerPredefinedPermissions.Update)]
        public IHttpActionResult UpdateContact(Contact contact)
        {
            return UpdateMember(contact);
        }

        /// <summary>
        /// Create organization
        /// </summary>
        [HttpPost]
        [Route("organizations")]
        [ResponseType(typeof(Organization))]
        [CheckPermission(Permission = CustomerPredefinedPermissions.Create)]
        public IHttpActionResult CreateOrganization(Organization organization)
        {
            return CreateMember(organization);
        }

        /// <summary>
        /// Update organization
        /// </summary>
        [HttpPut]
        [Route("organizations")]
        [ResponseType(typeof(void))]
        [CheckPermission(Permission = CustomerPredefinedPermissions.Update)]
        public IHttpActionResult UpdateOrganization(Organization organization)
        {
            return UpdateMember(organization);
        }

        /// <summary>
        /// Delete organizations
        /// </summary>
        /// <remarks>Delete organizations by given array of ids.</remarks>
        /// <param name="ids">An array of organizations ids</param>
        [HttpDelete]
        [Route("organizations")]
        [ResponseType(typeof(void))]
        [CheckPermission(Permission = CustomerPredefinedPermissions.Delete)]
        public IHttpActionResult DeleteOrganizations([FromUri] string[] ids)
        {
            return DeleteMembers(ids);
        }

        /// <summary>
        /// Delete contacts
        /// </summary>
        /// <remarks>Delete contacts by given array of ids.</remarks>
        /// <param name="ids">An array of contacts ids</param>
        [HttpDelete]
        [Route("contacts")]
        [ResponseType(typeof(void))]
        [CheckPermission(Permission = CustomerPredefinedPermissions.Delete)]
        public IHttpActionResult DeleteContacts([FromUri] string[] ids)
        {
            return DeleteMembers(ids);
        }

        /// <summary>
        /// Get organization
        /// </summary>
        /// <param name="id">Organization id</param>
        [HttpGet]
        [Route("organizations/{id}")]
        [ResponseType(typeof(Organization))]
        public IHttpActionResult GetOrganizationById(string id)
        {
            return GetMemberById(id);
        }

        /// <summary>
        /// Get contact
        /// </summary>
        /// <param name="id">Contact ID</param>
        [HttpGet]
        [Route("contacts/{id}")]
        [ResponseType(typeof(Contact))]
        public IHttpActionResult GetContactById(string id)
        {
            return GetMemberById(id);
        }


        /// <summary>
        /// Bulk get contacts 
        /// </summary>
        /// <param name="ids">contact IDs</param>
        [HttpGet]
        [Route("contacts")]
        [ResponseType(typeof(Contact[]))]
        public IHttpActionResult GetContactsByIds([FromUri]string[] ids)
        {
            return GetMembersByIds(ids);
        }

        /// <summary>
        /// Get vendor
        /// </summary>
        /// <param name="id">Vendor ID</param>
        [HttpGet]
        [Route("vendors/{id}")]
        [ResponseType(typeof(Vendor))]
        public IHttpActionResult GetVendorById(string id)
        {
            return GetMemberById(id);
        }

        /// <summary>
        /// Bulk get vendors 
        /// </summary>
        /// <param name="ids">Vendors IDs</param>
        [HttpGet]
        [Route("vendors")]
        [ResponseType(typeof(Vendor[]))]
        public IHttpActionResult GetVendorsByIds([FromUri]string[] ids)
        {
            return GetMembersByIds(ids);
        }

        /// <summary>
        /// Search vendors
        /// </summary>
        /// <remarks>Get array of vendors satisfied search criteria.</remarks>
        /// <param name="criteria">concrete instance of SearchCriteria type type will be created by using PolymorphicMemberSearchCriteriaJsonConverter</param>
        [HttpPost]
        [Route("vendors/search")]
        [ResponseType(typeof(VendorSearchResult))]
        public IHttpActionResult SearchVendors(MembersSearchCriteria criteria)
        {
            if (criteria == null)
            {
                criteria = new MembersSearchCriteria();
            }

            criteria.MemberType = typeof(Vendor).Name;
            var searchResult = _memberSearchService.SearchMembers(criteria);

            var result = new VendorSearchResult
            {
                TotalCount = searchResult.TotalCount,
                Vendors = searchResult.Results.OfType<Vendor>().ToList()
            };

            return Ok(result);
        }

        #endregion
    }
}
