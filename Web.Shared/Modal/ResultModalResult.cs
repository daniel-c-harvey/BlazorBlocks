//using NetBlocks.Models;

//namespace Web.Shared.Modal;

//public class ResultModalResult : NetBlocks.Models.ResultBase<ResultModalResult>
//{
//    public bool IsLoading { get; private set; } = true;

//    public override ResultModalResult Pass()
//    {
//        IsLoading = false;
//        return base.Pass();
//    }
    
//    public override ResultModalResult Fail(string message)
//    {
//        IsLoading = false;
//        return base.Fail(message);
//    }

//    public override ResultModalResult Warn(string message)
//    {
//        IsLoading = false;
//        return base.Warn(message);
//    }

//    public override ResultModalResult Inform(string message)
//    {
//        IsLoading = false;
//        return base.Inform(message);
//    }
    
//    public override ResultModalResult Merge(ResultModalResult result)
//    {
//        IsLoading = result.IsLoading;
//        return base.Merge(result);
//    }

//    public override ResultModalResult MergeInto(ResultModalResult result)
//    {
//        result.IsLoading = IsLoading;
//        return base.MergeInto(result);
//    }
    
//    public new static ResultModalResult From<TOther>(ResultBase<TOther> other) where TOther : ResultBase<TOther>, new()
//    {
//        var result = ResultBase<ResultModalResult>.From(other);
//        result.IsLoading = false;
//        return result;
//    }
    
//}