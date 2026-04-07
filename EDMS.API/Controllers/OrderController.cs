using EDMS.API.Models;
using EDMS.API.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace EDMS.API.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize(Policy = "ClinicalPolicy")]
public class OrderController : ControllerBase
{
    private readonly ClinicalService _clinicalService;

    public OrderController(ClinicalService clinicalService)
    {
        _clinicalService = clinicalService;
    }

    [HttpPost("verify")]
    public async Task<ActionResult<ApiResponse<AllergyCheckResult>>> Verify([FromBody] VerifyOrderRequest request)
    {
        var result = await _clinicalService.CheckConflictAsync(request.PatientId, request.DrugName);
        if (result.HasConflict)
        {
            return Conflict(new ApiResponse<AllergyCheckResult>
            {
                Success = false,
                Data = result,
                Message = "HardBlock: Allergy conflict detected."
            });
        }

        return Ok(new ApiResponse<AllergyCheckResult>
        {
            Success = true,
            Data = result,
            Message = "Order verified with no allergy conflict."
        });
    }
}
