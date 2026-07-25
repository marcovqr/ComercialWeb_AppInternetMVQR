using System;
using System.Collections.Generic;
using Comercial.Infrastructure.Persistence.Scaffolded.Entities;
using Microsoft.EntityFrameworkCore;

namespace Comercial.Infrastructure.Persistence.Scaffolded;

public partial class ScaffoldedComercialContext : DbContext
{
    public ScaffoldedComercialContext(DbContextOptions<ScaffoldedComercialContext> options)
        : base(options)
    {
    }

    public virtual DbSet<Compras_cab> Compras_cabs { get; set; }

    public virtual DbSet<Compras_det> Compras_dets { get; set; }

    public virtual DbSet<Instalacione> Instalaciones { get; set; }

    public virtual DbSet<cliente> clientes { get; set; }

    public virtual DbSet<factura_cab> factura_cabs { get; set; }

    public virtual DbSet<factura_det> factura_dets { get; set; }

    public virtual DbSet<marca> marcas { get; set; }

    public virtual DbSet<mensualidade> mensualidades { get; set; }

    public virtual DbSet<movimiento> movimientos { get; set; }

    public virtual DbSet<producto> productos { get; set; }

    public virtual DbSet<proveedore> proveedores { get; set; }

    public virtual DbSet<saldo_producto> saldo_productos { get; set; }

    public virtual DbSet<usuario> usuarios { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.UseCollation("Latin1_General_CI_AI");

        modelBuilder.Entity<Compras_cab>(entity =>
        {
            entity.HasKey(e => e.com_numero);

            entity.ToTable("Compras_cab");

            entity.Property(e => e.com_campo1).HasMaxLength(50);
            entity.Property(e => e.com_campo2).HasMaxLength(50);
            entity.Property(e => e.com_descuento).HasColumnType("decimal(18, 0)");
            entity.Property(e => e.com_estado).HasMaxLength(50);
            entity.Property(e => e.com_fecha).HasColumnType("datetime");
            entity.Property(e => e.com_impuesto).HasColumnType("decimal(18, 0)");
            entity.Property(e => e.com_subtotal).HasColumnType("decimal(18, 0)");
            entity.Property(e => e.com_total).HasColumnType("decimal(18, 0)");
            entity.Property(e => e.com_total_letras).HasMaxLength(500);
            entity.Property(e => e.pro_ciruc).HasMaxLength(13);
        });

        modelBuilder.Entity<Compras_det>(entity =>
        {
            entity.HasKey(e => new { e.com_numero, e.pro_codigp });

            entity.ToTable("Compras_det");

            entity.Property(e => e.com_campo1).HasMaxLength(50);
            entity.Property(e => e.com_cantidad).HasColumnType("decimal(18, 0)");
            entity.Property(e => e.com_estado).HasMaxLength(50);
            entity.Property(e => e.com_precio).HasColumnType("decimal(18, 0)");
        });

        modelBuilder.Entity<Instalacione>(entity =>
        {
            entity.HasKey(e => e.ins_codigo);

            entity.ToTable("Instalacione");

            entity.Property(e => e.ins_codigo).ValueGeneratedNever();
            entity.Property(e => e.cli_cedula).HasMaxLength(255);
            entity.Property(e => e.ins_descripcion_antena).HasMaxLength(255);
            entity.Property(e => e.ins_estado).HasMaxLength(255);
            entity.Property(e => e.ins_fecha_actual).HasColumnType("datetime");
            entity.Property(e => e.ins_fecha_creacion).HasMaxLength(255);
            entity.Property(e => e.ins_fecha_instalacion).HasColumnType("datetime");
            entity.Property(e => e.ins_fecha_mod).HasMaxLength(255);
            entity.Property(e => e.ins_ip_access_point).HasMaxLength(255);
            entity.Property(e => e.ins_ip_antena_wan).HasMaxLength(255);
            entity.Property(e => e.ins_ip_wan_router).HasMaxLength(255);
            entity.Property(e => e.ins_login_antena).HasMaxLength(255);
            entity.Property(e => e.ins_login_router).HasMaxLength(50);
            entity.Property(e => e.ins_observaciones).HasMaxLength(255);
            entity.Property(e => e.ins_password_antena).HasMaxLength(255);
            entity.Property(e => e.ins_password_router).HasMaxLength(255);
            entity.Property(e => e.ins_password_wifi).HasMaxLength(255);
        });

        modelBuilder.Entity<cliente>(entity =>
        {
            entity.HasKey(e => e.cli_cedula).HasName("PK__clientes__B42D878AAFD9261A");

            entity.Property(e => e.cli_cedula)
                .HasMaxLength(13)
                .IsUnicode(false);
            entity.Property(e => e.cli_apellidos)
                .HasMaxLength(50)
                .IsUnicode(false);
            entity.Property(e => e.cli_direccion)
                .HasMaxLength(150)
                .IsUnicode(false);
            entity.Property(e => e.cli_email)
                .HasMaxLength(50)
                .IsUnicode(false)
                .HasDefaultValueSql("(NULL)");
            entity.Property(e => e.cli_estado)
                .HasMaxLength(10)
                .IsUnicode(false);
            entity.Property(e => e.cli_fechamod)
                .HasDefaultValueSql("(getdate())")
                .HasColumnType("datetime");
            entity.Property(e => e.cli_nombres)
                .HasMaxLength(50)
                .IsUnicode(false);
            entity.Property(e => e.cli_obser)
                .HasMaxLength(100)
                .IsUnicode(false)
                .HasDefaultValueSql("(NULL)");
            entity.Property(e => e.cli_telefono)
                .HasMaxLength(10)
                .IsUnicode(false)
                .HasDefaultValueSql("(NULL)");
        });

        modelBuilder.Entity<factura_cab>(entity =>
        {
            entity.HasKey(e => e.fac_numero);

            entity.ToTable("factura_cab");

            entity.Property(e => e.fac_numero).ValueGeneratedNever();
            entity.Property(e => e.cli_ciruc)
                .HasMaxLength(13)
                .IsUnicode(false);
            entity.Property(e => e.fac_descuento).HasColumnType("decimal(15, 4)");
            entity.Property(e => e.fac_estado)
                .HasMaxLength(5)
                .IsUnicode(false);
            entity.Property(e => e.fac_fecha)
                .HasDefaultValueSql("(getdate())")
                .HasColumnType("datetime");
            entity.Property(e => e.fac_impuesto).HasColumnType("decimal(15, 4)");
            entity.Property(e => e.fac_subtotal).HasColumnType("decimal(18, 4)");
            entity.Property(e => e.fac_tipo)
                .HasMaxLength(2)
                .IsUnicode(false);
            entity.Property(e => e.fac_total).HasColumnType("decimal(15, 4)");

            entity.HasOne(d => d.cli_cirucNavigation).WithMany(p => p.factura_cabs)
                .HasForeignKey(d => d.cli_ciruc)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FacCAb_Clientes");
        });

        modelBuilder.Entity<factura_det>(entity =>
        {
            entity.HasKey(e => new { e.fac_numero, e.pro_codigo }).HasName("PK__factura___D6022E5A6C12FEE2");

            entity.ToTable("factura_det");

            entity.Property(e => e.fac_cantidad).HasColumnType("decimal(15, 4)");
            entity.Property(e => e.fac_estado)
                .HasMaxLength(80)
                .IsUnicode(false);
            entity.Property(e => e.fac_precio).HasColumnType("decimal(15, 4)");

            entity.HasOne(d => d.fac_numeroNavigation).WithMany(p => p.factura_dets)
                .HasForeignKey(d => d.fac_numero)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("facDet_facCab");

            entity.HasOne(d => d.pro_codigoNavigation).WithMany(p => p.factura_dets)
                .HasForeignKey(d => d.pro_codigo)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("dacDet_Productos");
        });

        modelBuilder.Entity<marca>(entity =>
        {
            entity.HasKey(e => e.mar_codigo);

            entity.Property(e => e.mar_descripcion)
                .HasMaxLength(50)
                .IsUnicode(false);
            entity.Property(e => e.mar_estado)
                .HasMaxLength(10)
                .IsUnicode(false);
            entity.Property(e => e.mar_modelo)
                .HasMaxLength(50)
                .IsUnicode(false);
        });

        modelBuilder.Entity<mensualidade>(entity =>
        {
            entity.HasKey(e => e.men_codigo).HasName("PK__mensuali__409BBDECF2400D03");

            entity.Property(e => e.cli_cedula)
                .HasMaxLength(13)
                .IsUnicode(false);
            entity.Property(e => e.men_cuota).HasColumnType("decimal(18, 0)");
            entity.Property(e => e.men_estado)
                .HasMaxLength(10)
                .IsUnicode(false);
            entity.Property(e => e.men_fechadesde)
                .HasMaxLength(50)
                .IsUnicode(false);
            entity.Property(e => e.men_fechahasta)
                .HasMaxLength(50)
                .IsUnicode(false);
        });

        modelBuilder.Entity<movimiento>(entity =>
        {
            entity.HasKey(e => e.mov_id).HasName("PK__movimien__D1BE75C705C40072");

            entity.Property(e => e.mov_id).ValueGeneratedNever();
            entity.Property(e => e.mov_campo1)
                .HasMaxLength(50)
                .IsUnicode(false)
                .HasDefaultValueSql("(NULL)");
            entity.Property(e => e.mov_campo2)
                .HasMaxLength(50)
                .IsUnicode(false)
                .HasDefaultValueSql("(NULL)");
            entity.Property(e => e.mov_campo3)
                .HasMaxLength(50)
                .IsUnicode(false)
                .HasDefaultValueSql("(NULL)");
            entity.Property(e => e.mov_cantidad).HasColumnType("decimal(15, 4)");
            entity.Property(e => e.mov_costo).HasColumnType("decimal(15, 4)");
            entity.Property(e => e.mov_estado)
                .HasMaxLength(50)
                .IsUnicode(false);
            entity.Property(e => e.mov_fecha).HasColumnType("datetime");
            entity.Property(e => e.mov_nombre)
                .HasMaxLength(50)
                .IsUnicode(false);
            entity.Property(e => e.mov_observaciones)
                .HasMaxLength(50)
                .IsUnicode(false)
                .HasDefaultValueSql("(NULL)");
        });

        modelBuilder.Entity<producto>(entity =>
        {
            entity.HasKey(e => e.pro_codigo).HasName("PK__producto__25CC172EAC2EFD9A");

            entity.Property(e => e.pro_codigo).ValueGeneratedNever();
            entity.Property(e => e.pro_descripcion)
                .HasMaxLength(50)
                .IsUnicode(false);
            entity.Property(e => e.pro_estado)
                .HasMaxLength(10)
                .IsUnicode(false);
            entity.Property(e => e.pro_fechamod)
                .HasDefaultValueSql("(getdate())")
                .HasColumnType("datetime");
            entity.Property(e => e.pro_imei)
                .HasMaxLength(15)
                .IsUnicode(false);
            entity.Property(e => e.pro_modelo)
                .HasMaxLength(30)
                .IsUnicode(false);
            entity.Property(e => e.pro_obser)
                .HasMaxLength(150)
                .IsUnicode(false)
                .HasDefaultValueSql("(NULL)");
            entity.Property(e => e.pro_precio).HasColumnType("decimal(15, 4)");
            entity.Property(e => e.pro_unidad)
                .HasMaxLength(10)
                .IsUnicode(false);
        });

        modelBuilder.Entity<proveedore>(entity =>
        {
            entity.HasKey(e => e.prov_ciruc).HasName("PK__proveedo__CF58DCF070D7F7D8");

            entity.Property(e => e.prov_ciruc)
                .HasMaxLength(13)
                .IsUnicode(false);
            entity.Property(e => e.prov_apellidos)
                .HasMaxLength(50)
                .IsUnicode(false);
            entity.Property(e => e.prov_campo4)
                .HasMaxLength(50)
                .IsUnicode(false)
                .HasDefaultValueSql("(NULL)");
            entity.Property(e => e.prov_campo5)
                .HasMaxLength(50)
                .IsUnicode(false)
                .HasDefaultValueSql("(NULL)");
            entity.Property(e => e.prov_celular)
                .HasMaxLength(25)
                .IsUnicode(false)
                .HasDefaultValueSql("(NULL)");
            entity.Property(e => e.prov_direccion)
                .HasMaxLength(200)
                .IsUnicode(false);
            entity.Property(e => e.prov_email)
                .HasMaxLength(50)
                .IsUnicode(false);
            entity.Property(e => e.prov_estado)
                .HasMaxLength(25)
                .IsUnicode(false);
            entity.Property(e => e.prov_fcreacion)
                .HasDefaultValueSql("(NULL)")
                .HasColumnType("datetime");
            entity.Property(e => e.prov_fmodif)
                .HasDefaultValueSql("(NULL)")
                .HasColumnType("datetime");
            entity.Property(e => e.prov_fpago)
                .HasMaxLength(30)
                .IsUnicode(false);
            entity.Property(e => e.prov_nombres)
                .HasMaxLength(50)
                .IsUnicode(false);
            entity.Property(e => e.prov_observaciones)
                .HasMaxLength(300)
                .IsUnicode(false)
                .HasDefaultValueSql("(NULL)");
            entity.Property(e => e.prov_razonsocial)
                .HasMaxLength(100)
                .IsUnicode(false);
            entity.Property(e => e.prov_telefono)
                .HasMaxLength(10)
                .IsUnicode(false)
                .HasDefaultValueSql("(NULL)");
            entity.Property(e => e.usu_cedula)
                .HasMaxLength(10)
                .IsUnicode(false);
        });

        modelBuilder.Entity<saldo_producto>(entity =>
        {
            entity.HasKey(e => e.sal_id).HasName("PK__saldo_pr__FEF11768E2908135");

            entity.ToTable("saldo_producto");

            entity.Property(e => e.sal_id).ValueGeneratedNever();
            entity.Property(e => e.sal_estado)
                .HasMaxLength(25)
                .IsUnicode(false);
            entity.Property(e => e.sal_fecha).HasColumnType("datetime");
            entity.Property(e => e.sal_producto).HasColumnType("decimal(15, 4)");
        });

        modelBuilder.Entity<usuario>(entity =>
        {
            entity.HasKey(e => e.usu_cedula).HasName("PK__usuarios__A0704FC26DBFE49C");

            entity.Property(e => e.usu_cedula)
                .HasMaxLength(10)
                .IsUnicode(false);
            entity.Property(e => e.usu_estado)
                .HasMaxLength(10)
                .IsUnicode(false);
            entity.Property(e => e.usu_login)
                .HasMaxLength(10)
                .IsUnicode(false);
            entity.Property(e => e.usu_obser)
                .HasMaxLength(50)
                .IsUnicode(false)
                .HasDefaultValueSql("(NULL)");
            entity.Property(e => e.usu_pass)
                .HasMaxLength(20)
                .IsUnicode(false);
        });

        OnModelCreatingPartial(modelBuilder);
    }

    partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
}
