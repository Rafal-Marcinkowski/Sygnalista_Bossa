using Library;

namespace PogromcaBossa.LocalLibrary.Services;

public static class CompanyInfoManager
{
    public static string CompanyCode { get; set; }
    public static string MedianaText { get; set; }

    public async static Task CreateCompanyCode(string title)
    {
        CompanyCode = await NameTranslation.ConvertHeaderToTranslation(title);
        _ = SaveTextToFile.SaveAsync("CompanyCode", CompanyCode);
    }

    public async static Task CreateMedianaText()
    {
        if (CompanyCode is not null)
        {
            MedianaText = await NameTranslation.GetTurnoverMedianForCompany(CompanyCode);
        }
    }
}
