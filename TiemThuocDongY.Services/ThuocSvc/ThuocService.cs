using System.Collections.Generic;
using TiemThuocDongY.Data.Repositories;
using TiemThuocDongY.Domain.Entities;

namespace TiemThuocDongY.Services.ThuocSvc;

public class ThuocService
{
    private readonly ThuocRepository _repo;

    public ThuocService(ThuocRepository repo)
    {
        _repo = repo;
    }

    public IList<Thuoc> GetAll()
        => _repo.GetAll();

    public Thuoc Add(Thuoc thuoc)
    {
        var id = _repo.Insert(thuoc);
        thuoc.ThuocId = id;
        return thuoc;
    }

    public bool Update(Thuoc thuoc)
    {
        var rows = _repo.Update(thuoc);
        return rows > 0;
    }

    public void Delete(int thuocId)
        => _repo.SoftDelete(thuocId);
}
