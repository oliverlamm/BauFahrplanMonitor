using System;
using System.Collections.Generic;
using BauFahrplanMonitor.Core.Models;
using Microsoft.EntityFrameworkCore;

namespace BauFahrplanMonitor.Core.Data;

public partial class UjBauDbContext : DbContext
{
    public UjBauDbContext(DbContextOptions<UjBauDbContext> options)
        : base(options)
    {
    }

    public virtual DbSet<BasisBetriebsstelle> BasisBetriebsstelle { get; set; }

    public virtual DbSet<BasisBetriebsstelle2strecke> BasisBetriebsstelle2strecke { get; set; }

    public virtual DbSet<BasisBetriebsstelleTyp> BasisBetriebsstelleTyp { get; set; }

    public virtual DbSet<BasisBetriebsstellenbereich> BasisBetriebsstellenbereich { get; set; }

    public virtual DbSet<BasisKunde> BasisKunde { get; set; }

    public virtual DbSet<BasisNetz> BasisNetz { get; set; }

    public virtual DbSet<BasisNetzbezirk> BasisNetzbezirk { get; set; }

    public virtual DbSet<BasisRegion> BasisRegion { get; set; }

    public virtual DbSet<BasisStrecke> BasisStrecke { get; set; }

    public virtual DbSet<BasisStreckeAbschnitt> BasisStreckeAbschnitt { get; set; }

    public virtual DbSet<BasisStreckeInfo> BasisStreckeInfo { get; set; }

    public virtual DbSet<BasisTriebfahrzeuge> BasisTriebfahrzeuge { get; set; }

    public virtual DbSet<BbpneoMassnahme> BbpneoMassnahme { get; set; }

    public virtual DbSet<BbpneoMassnahmeRegelung> BbpneoMassnahmeRegelung { get; set; }

    public virtual DbSet<BbpneoMassnahmeRegelungBve> BbpneoMassnahmeRegelungBve { get; set; }

    public virtual DbSet<BbpneoMassnahmeRegelungBveAps> BbpneoMassnahmeRegelungBveAps { get; set; }

    public virtual DbSet<BbpneoMassnahmeRegelungBveIav> BbpneoMassnahmeRegelungBveIav { get; set; }

    public virtual DbSet<FploDokument> FploDokument { get; set; }

    public virtual DbSet<FploDokumentStreckenabschnitte> FploDokumentStreckenabschnitte { get; set; }

    public virtual DbSet<FploDokumentZug> FploDokumentZug { get; set; }

    public virtual DbSet<FploDokumentZugFahrplan> FploDokumentZugFahrplan { get; set; }

    public virtual DbSet<FploDokumentZugRegelung> FploDokumentZugRegelung { get; set; }

    public virtual DbSet<NfplZug> NfplZug { get; set; }

    public virtual DbSet<NfplZugVariante> NfplZugVariante { get; set; }

    public virtual DbSet<NfplZugVarianteVerlauf> NfplZugVarianteVerlauf { get; set; }

    public virtual DbSet<SchemaVersion> SchemaVersion { get; set; }

    public virtual DbSet<UebDokument> UebDokument { get; set; }

    public virtual DbSet<UebDokumentStreckenabschnitte> UebDokumentStreckenabschnitte { get; set; }

    public virtual DbSet<UebDokumentZug> UebDokumentZug { get; set; }

    public virtual DbSet<UebDokumentZugKnotenzeiten> UebDokumentZugKnotenzeiten { get; set; }

    public virtual DbSet<UebDokumentZugRegelung> UebDokumentZugRegelung { get; set; }

    public virtual DbSet<UjbauSender> UjbauSender { get; set; }

    public virtual DbSet<UjbauVorgang> UjbauVorgang { get; set; }

    public virtual DbSet<UjbauVorgangBbmn> UjbauVorgangBbmn { get; set; }

    public virtual DbSet<ZvfDokument> ZvfDokument { get; set; }

    public virtual DbSet<ZvfDokumentStreckenabschnitte> ZvfDokumentStreckenabschnitte { get; set; }

    public virtual DbSet<ZvfDokumentZug> ZvfDokumentZug { get; set; }

    public virtual DbSet<ZvfDokumentZugAbweichung> ZvfDokumentZugAbweichung { get; set; }

    public virtual DbSet<ZvfDokumentZugEntfallen> ZvfDokumentZugEntfallen { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.HasPostgresExtension("postgis");

        modelBuilder.Entity<BasisBetriebsstelle>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("basis_betriebsstelle_pkey");

            entity.Property(e => e.IstBasisDatensatz).HasDefaultValue(false);
            entity.Property(e => e.NetzbezirkRef).HasDefaultValue(0L);
            entity.Property(e => e.RegionRef).HasDefaultValue(0L);
            entity.Property(e => e.TypRef).HasDefaultValue(0L);
            entity.Property(e => e.UpdatedAt).HasDefaultValueSql("now()");

            entity.HasOne(d => d.NetzbezirkRefNavigation).WithMany(p => p.BasisBetriebsstelle)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("basis_betriebsstelle_netzbezirk_ref_fkey");

            entity.HasOne(d => d.RegionRefNavigation).WithMany(p => p.BasisBetriebsstelle)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("basis_betriebsstelle_region_ref_fkey");

            entity.HasOne(d => d.TypRefNavigation).WithMany(p => p.BasisBetriebsstelle)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("basis_betriebsstelle_typ_ref_fkey");
        });

        modelBuilder.Entity<BasisBetriebsstelle2strecke>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("bst2str_pkey");

            entity.Property(e => e.IstBasisDatensatz).HasDefaultValue(false);
            entity.Property(e => e.UpdatedAt).HasDefaultValueSql("now()");

            entity.HasOne(d => d.BstRefNavigation).WithMany(p => p.BasisBetriebsstelle2strecke)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("basis_betriebsstelle2strecke_bst_ref_fkey");

            entity.HasOne(d => d.StreckeRefNavigation).WithMany(p => p.BasisBetriebsstelle2strecke)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("basis_betriebsstelle2strecke_strecke_ref_fkey");
        });

        modelBuilder.Entity<BasisBetriebsstelleTyp>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("bst_typ_pkey");

            entity.Property(e => e.Id).HasDefaultValueSql("nextval('ujbaudb.newtable_id_seq'::regclass)");
            entity.Property(e => e.IstBasisDatensatz).HasDefaultValue(false);
            entity.Property(e => e.UpdatedAt).HasDefaultValueSql("now()");
        });

        modelBuilder.Entity<BasisBetriebsstellenbereich>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("basis_betriebsstellenebreich_pk");

            entity.Property(e => e.Id).HasDefaultValueSql("nextval('ujbaudb.basis_betriebsstellenebreich_id_seq'::regclass)");

            entity.HasOne(d => d.BstChildRefNavigation).WithMany(p => p.BasisBetriebsstellenbereichBstChildRefNavigation)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("basis_betriebsstellenebreich_basis_betriebsstelle_fk_1");

            entity.HasOne(d => d.BstRefNavigation).WithMany(p => p.BasisBetriebsstellenbereichBstRefNavigation).HasConstraintName("basis_betriebsstellenebreich_basis_betriebsstelle_fk");
        });

        modelBuilder.Entity<BasisKunde>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("kunde_pkey");

            entity.Property(e => e.IstBasisDatensatz).HasDefaultValue(false);
            entity.Property(e => e.UpdatedAt).HasDefaultValueSql("now()");
        });

        modelBuilder.Entity<BasisNetz>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("basis_netz_pkey");

            entity.Property(e => e.IstBasisDatensatz).HasDefaultValue(false);
            entity.Property(e => e.UpdatedAt).HasDefaultValueSql("now()");
        });

        modelBuilder.Entity<BasisNetzbezirk>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("basis_netzbezirk_pkey");

            entity.Property(e => e.IstBasisDatensatz).HasDefaultValue(false);
            entity.Property(e => e.NetzRef).HasDefaultValue(0L);
            entity.Property(e => e.UpdatedAt).HasDefaultValueSql("now()");

            entity.HasOne(d => d.NetzRefNavigation).WithMany(p => p.BasisNetzbezirk).HasConstraintName("basis_netzbezirk_netz_ref_fkey");
        });

        modelBuilder.Entity<BasisRegion>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("basis_region_pkey");

            entity.Property(e => e.IstBasisDatensatz).HasDefaultValue(false);
            entity.Property(e => e.UpdatedAt).HasDefaultValueSql("now()");
        });

        modelBuilder.Entity<BasisStrecke>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("basis_strecke_pkey");

            entity.Property(e => e.IstBasisDatensatz).HasDefaultValue(false);
            entity.Property(e => e.UpdatedAt).HasDefaultValueSql("now()");
        });

        modelBuilder.Entity<BasisStreckeAbschnitt>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("basis_strecke_abschnitt_pk");

            entity.HasOne(d => d.BisBstRefNavigation).WithMany(p => p.BasisStreckeAbschnittBisBstRefNavigation)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("basis_strecke_abschnitt_basis_betriebsstelle_fk_1");

            entity.HasOne(d => d.StreckeRefNavigation).WithMany(p => p.BasisStreckeAbschnitt).HasConstraintName("basis_strecke_abschnitt_basis_strecke_fk");

            entity.HasOne(d => d.VonBstRefNavigation).WithMany(p => p.BasisStreckeAbschnittVonBstRefNavigation)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("basis_strecke_abschnitt_basis_betriebsstelle_fk");
        });

        modelBuilder.Entity<BasisStreckeInfo>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("basis_strecke_info_pkey");

            entity.Property(e => e.Id).HasDefaultValueSql("nextval('ujbaudb.basis_strecken_info_id_seq'::regclass)");
            entity.Property(e => e.IstBasisDatensatz).HasDefaultValue(false);
            entity.Property(e => e.NetzRef).HasDefaultValue(0L);
            entity.Property(e => e.RegionRef).HasDefaultValue(0L);
            entity.Property(e => e.Richtung).HasDefaultValue(0L);
            entity.Property(e => e.StreckenRef).HasDefaultValue(0L);
            entity.Property(e => e.UpdatedAt).HasDefaultValueSql("now()");

            entity.HasOne(d => d.NetzRefNavigation).WithMany(p => p.BasisStreckeInfo)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("basis_strecke_info_netz_ref_fkey");

            entity.HasOne(d => d.RegionRefNavigation).WithMany(p => p.BasisStreckeInfo)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("basis_strecke_info_region_ref_fkey");

            entity.HasOne(d => d.StreckenRefNavigation).WithMany(p => p.BasisStreckeInfo).HasConstraintName("basis_strecke_info_strecken_ref_fkey");
        });

        modelBuilder.Entity<BasisTriebfahrzeuge>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("basis_triebfahrzeuge_pk");

            entity.Property(e => e.AktiveNeigetechnik).HasDefaultValue(false);
            entity.Property(e => e.Elektrifiziert).HasDefaultValue(false);
            entity.Property(e => e.Triebwagen).HasDefaultValue(false);
        });

        modelBuilder.Entity<BbpneoMassnahme>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("bbpneo_massnahme_pkey");

            entity.Property(e => e.Aktiv).HasDefaultValue(false);
            entity.Property(e => e.UpdatedAt).HasDefaultValueSql("now()");

            entity.HasOne(d => d.MasBisBst2strRefNavigation).WithMany(p => p.BbpneoMassnahmeMasBisBst2strRefNavigation)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("bbpneo_massnahme_mas_bis_bst2str_ref_fkey");

            entity.HasOne(d => d.MasVonBst2strRefNavigation).WithMany(p => p.BbpneoMassnahmeMasVonBst2strRefNavigation)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("bbpneo_massnahme_mas_von_bst2str_ref_fkey");

            entity.HasOne(d => d.RegionRefNavigation).WithMany(p => p.BbpneoMassnahme)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("bbpneo_massnahme_region_ref_fkey");
        });

        modelBuilder.Entity<BbpneoMassnahmeRegelung>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("bbpneo_massnahme_regelung_pkey");

            entity.Property(e => e.Aktiv).HasDefaultValue(true);
            entity.Property(e => e.Durchgehend).HasDefaultValue(false);
            entity.Property(e => e.Schichtweise).HasDefaultValue(false);
            entity.Property(e => e.UpdatedAt).HasDefaultValueSql("now()");

            entity.HasOne(d => d.BbpneoMasRefNavigation).WithMany(p => p.BbpneoMassnahmeRegelung).HasConstraintName("bbpneo_massnahme_regelung_bbpneo_mas_ref_fkey");

            entity.HasOne(d => d.Bst2strBisRefNavigation).WithMany(p => p.BbpneoMassnahmeRegelungBst2strBisRefNavigation)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("bbpneo_massnahme_regelung_bst2str_bis_ref_fkey");

            entity.HasOne(d => d.Bst2strVonRefNavigation).WithMany(p => p.BbpneoMassnahmeRegelungBst2strVonRefNavigation)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("bbpneo_massnahme_regelung_bst2str_von_ref_fkey");
        });

        modelBuilder.Entity<BbpneoMassnahmeRegelungBve>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("bbpneo_massnahme_regelung_bve_pkey");

            entity.Property(e => e.Id).HasDefaultValueSql("nextval('ujbaudb.bbpneo_masnahme_regelung_bve_id_seq'::regclass)");
            entity.Property(e => e.Aktiv).HasDefaultValue(true);
            entity.Property(e => e.ApsBetroffenheit).HasDefaultValue(false);
            entity.Property(e => e.ApsFreiVonFahrzeugen).HasDefaultValue(false);
            entity.Property(e => e.IavBetroffenheit).HasDefaultValue(false);
            entity.Property(e => e.UpdatedAt).HasDefaultValueSql("now()");

            entity.HasOne(d => d.BbpneoMasRegRefNavigation).WithMany(p => p.BbpneoMassnahmeRegelungBve).HasConstraintName("bbpneo_massnahme_regelung_bve_bbpneo_massnahme_regelung_fk");

            entity.HasOne(d => d.Bst2strBisRefNavigation).WithMany(p => p.BbpneoMassnahmeRegelungBveBst2strBisRefNavigation)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("bbpneo_massnahme_regelung_bve_bst2str_bis_ref_fkey");

            entity.HasOne(d => d.Bst2strVonRefNavigation).WithMany(p => p.BbpneoMassnahmeRegelungBveBst2strVonRefNavigation)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("bbpneo_massnahme_regelung_bve_bst2str_von_ref_fkey");
        });

        modelBuilder.Entity<BbpneoMassnahmeRegelungBveAps>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("bbpneo_massnahme_regelung_bve_aps_pkey");

            entity.Property(e => e.Oberleitung).HasDefaultValue(false);
            entity.Property(e => e.OberleitungAus).HasDefaultValue(false);
            entity.Property(e => e.UpdatedAt).HasDefaultValueSql("now()");

            entity.HasOne(d => d.BbpneoMassnahmeRegelungBveRefNavigation).WithMany(p => p.BbpneoMassnahmeRegelungBveAps).HasConstraintName("fk_bve_aps_bve_ref_massnahme_regelung_bve");

            entity.HasOne(d => d.BstRefNavigation).WithMany(p => p.BbpneoMassnahmeRegelungBveAps).HasConstraintName("fk_bve_aps_bst_ref_basis_betriebsstelle");
        });

        modelBuilder.Entity<BbpneoMassnahmeRegelungBveIav>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("bbpneo_massnahme_regelung_bve_iav_pkey");

            entity.Property(e => e.Oberleitung).HasDefaultValue(false);
            entity.Property(e => e.OberleitungAus).HasDefaultValue(false);
            entity.Property(e => e.UpdatedAt).HasDefaultValueSql("now()");

            entity.HasOne(d => d.BbpneoMassnahmeRegelungBveRefNavigation).WithMany(p => p.BbpneoMassnahmeRegelungBveIav).HasConstraintName("fk_bve_iav_bve_ref_massnahme_regelung_bve");

            entity.HasOne(d => d.Bst2strRefNavigation).WithMany(p => p.BbpneoMassnahmeRegelungBveIav)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("fk_bve_iav_bst2str_ref_basis_betriebsstelle2strecke");
        });

        modelBuilder.Entity<FploDokument>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("fplo_dokument_pkey");

            entity.Property(e => e.IstEntwurf).HasDefaultValue(false);
            entity.Property(e => e.IstNachtrag).HasDefaultValue(false);
            entity.Property(e => e.IstTeillieferung).HasDefaultValue(false);
            entity.Property(e => e.MasterRegionRef).HasDefaultValue(0L);
            entity.Property(e => e.RegionRef).HasDefaultValue(0L);
            entity.Property(e => e.SenderRef).HasDefaultValue(0L);
            entity.Property(e => e.UjbauVorgangRef).HasDefaultValue(0L);
            entity.Property(e => e.UpdatedAt).HasDefaultValueSql("now()");
            entity.Property(e => e.Version).HasDefaultValue(0L);
            entity.Property(e => e.VersionMajor).HasDefaultValue(0L);
            entity.Property(e => e.VersionMinor).HasDefaultValue(0L);
            entity.Property(e => e.VersionSub).HasDefaultValue(0L);

            entity.HasOne(d => d.MasterRegionRefNavigation).WithMany(p => p.FploDokumentMasterRegionRefNavigation)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("fplo_dokument_master_region_ref_fkey");

            entity.HasOne(d => d.RegionRefNavigation).WithMany(p => p.FploDokumentRegionRefNavigation)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("fplo_dokument_region_ref_fkey");

            entity.HasOne(d => d.SenderRefNavigation).WithMany(p => p.FploDokument)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("fplo_dokument_sender_ref_fkey");

            entity.HasOne(d => d.UjbauVorgangRefNavigation).WithMany(p => p.FploDokument).HasConstraintName("fplo_dokument_ujbau_vorgang_ref_fkey");
        });

        modelBuilder.Entity<FploDokumentStreckenabschnitte>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("pk_fplo_strabs");

            entity.Property(e => e.UpdatedAt).HasDefaultValueSql("now()");

            entity.HasOne(d => d.FploDokumentRefNavigation).WithMany(p => p.FploDokumentStreckenabschnitte).HasConstraintName("fk_fplo_dok_strabs_dok");
        });

        modelBuilder.Entity<FploDokumentZug>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("fplo_dokument_zug_pkey");

            entity.Property(e => e.Bedarf).HasDefaultValue(false);
            entity.Property(e => e.Ebula).HasDefaultValue(false);
            entity.Property(e => e.FploDokumentRef).HasDefaultValue(0L);
            entity.Property(e => e.KundeRef).HasDefaultValue(0L);
            entity.Property(e => e.Lauterzug).HasDefaultValue(false);
            entity.Property(e => e.RegelwegAbBstRef).HasDefaultValue(0L);
            entity.Property(e => e.RegelwegZielBstRef).HasDefaultValue(0L);
            entity.Property(e => e.Sicherheitsrelevant).HasDefaultValue(false);
            entity.Property(e => e.UpdatedAt).HasDefaultValueSql("now()");

            entity.HasOne(d => d.FploDokumentRefNavigation).WithMany(p => p.FploDokumentZug).HasConstraintName("fplo_dokument_zug_fplo_dokument_ref_fkey");

            entity.HasOne(d => d.KundeRefNavigation).WithMany(p => p.FploDokumentZug)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("fplo_dokument_zug_kunde_ref_fkey");

            entity.HasOne(d => d.RegelwegAbBstRefNavigation).WithMany(p => p.FploDokumentZugRegelwegAbBstRefNavigation)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("fplo_dokument_zug_regelweg_ab_bst_ref_fkey");

            entity.HasOne(d => d.RegelwegZielBstRefNavigation).WithMany(p => p.FploDokumentZugRegelwegZielBstRefNavigation)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("fplo_dokument_zug_regelweg_ziel_bst_ref_fkey");
        });

        modelBuilder.Entity<FploDokumentZugFahrplan>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("fplo_dokument_zug_fahrplan_pkey");

            entity.Property(e => e.FploDokumentZugRef).HasDefaultValue(0L);
            entity.Property(e => e.Lfdnr).HasDefaultValue(1L);
            entity.Property(e => e.UpdatedAt).HasDefaultValueSql("now()");

            entity.HasOne(d => d.BstRefNavigation).WithMany(p => p.FploDokumentZugFahrplan).HasConstraintName("fplo_dokument_zug_fahrplan_bst_ref_fkey");

            entity.HasOne(d => d.FploDokumentZugRefNavigation).WithMany(p => p.FploDokumentZugFahrplan).HasConstraintName("fplo_dokument_zug_fahrplan_fplo_dokument_zug_ref_fkey");
        });

        modelBuilder.Entity<FploDokumentZugRegelung>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("pk_fplo_zugreg");

            entity.Property(e => e.AnkerBstRef).HasDefaultValue(0L);
            entity.Property(e => e.UpdatedAt).HasDefaultValueSql("now()");

            entity.HasOne(d => d.AnkerBstRefNavigation).WithMany(p => p.FploDokumentZugRegelung)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("fk_fplo_zreg_bst");

            entity.HasOne(d => d.FploDokumentZugRefNavigation).WithMany(p => p.FploDokumentZugRegelung).HasConstraintName("fk_fplo_zreg_zug");
        });

        modelBuilder.Entity<NfplZug>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("nfpl_zug_pk");

            entity.Property(e => e.UpdatedAt).HasDefaultValueSql("now()");
        });

        modelBuilder.Entity<NfplZugVariante>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("nfpl_zug_variante_pk");

            entity.Property(e => e.RegionRef).HasDefaultValue(0L);
            entity.Property(e => e.UpdatedAt).HasDefaultValueSql("now()");

            entity.HasOne(d => d.NfplZugRefNavigation).WithMany(p => p.NfplZugVariante).HasConstraintName("nfpl_zug_variante_nfpl_zug_fk");

            entity.HasOne(d => d.RegionRefNavigation).WithMany(p => p.NfplZugVariante)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("nfpl_zug_variante_basis_region_fk");
        });

        modelBuilder.Entity<NfplZugVarianteVerlauf>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("nfpl_zug_variante_verlauf_pk");

            entity.Property(e => e.NfplZugVarRef).HasDefaultValue(0L);
            entity.Property(e => e.UpdatedAt).HasDefaultValueSql("now()");

            entity.HasOne(d => d.BstRefNavigation).WithMany(p => p.NfplZugVarianteVerlauf)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("nfpl_zug_variante_verlauf_basis_betriebsstelle_fk");

            entity.HasOne(d => d.NfplZugVarRefNavigation).WithMany(p => p.NfplZugVarianteVerlauf).HasConstraintName("nfpl_zug_var_verlauf_nfpl_zug_variante_fk");
        });

        modelBuilder.Entity<SchemaVersion>(entity =>
        {
            entity.HasKey(e => e.Schema).HasName("schema_version_pkey");

            entity.Property(e => e.Schema).ValueGeneratedNever();
            entity.Property(e => e.UpdatedAt).HasDefaultValueSql("now()");
        });

        modelBuilder.Entity<UebDokument>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("pk_ueb_dok");

            entity.Property(e => e.SenderRef).HasDefaultValue(0L);
            entity.Property(e => e.UpdatedAt).HasDefaultValueSql("now()");
            entity.Property(e => e.Version).HasDefaultValue(0L);
            entity.Property(e => e.VersionMajor).HasDefaultValue(0L);
            entity.Property(e => e.VersionMinor).HasDefaultValue(0L);
            entity.Property(e => e.VersionSub).HasDefaultValue(0L);

            entity.HasOne(d => d.RegionRefNavigation).WithMany(p => p.UebDokument).HasConstraintName("fk_ueb_dok_region_ref");

            entity.HasOne(d => d.SenderRefNavigation).WithMany(p => p.UebDokument)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("fk_ueb_dok_sender_ref");

            entity.HasOne(d => d.UjbauVorgangRefNavigation).WithMany(p => p.UebDokument).HasConstraintName("fk_ueb_dok_vorgang_ref");
        });

        modelBuilder.Entity<UebDokumentStreckenabschnitte>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("pk_ueb_strabs");

            entity.Property(e => e.UpdatedAt).HasDefaultValueSql("now()");

            entity.HasOne(d => d.UebDokumentRefNavigation).WithMany(p => p.UebDokumentStreckenabschnitte).HasConstraintName("fk_ueb_dok_strabs_dok");
        });

        modelBuilder.Entity<UebDokumentZug>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("ueb_dokument_zug_pkey");

            entity.Property(e => e.Bedarf).HasDefaultValue(false);
            entity.Property(e => e.Ebula).HasDefaultValue(false);
            entity.Property(e => e.KundeRef).HasDefaultValue(0L);
            entity.Property(e => e.Lauterzug).HasDefaultValue(false);
            entity.Property(e => e.RegelwegAbBstRef).HasDefaultValue(0L);
            entity.Property(e => e.RegelwegZielBstRef).HasDefaultValue(0L);
            entity.Property(e => e.Sicherheitsrelevant).HasDefaultValue(false);
            entity.Property(e => e.UebDokumentRef).HasDefaultValue(0L);
            entity.Property(e => e.UpdatedAt).HasDefaultValueSql("now()");

            entity.HasOne(d => d.KundeRefNavigation).WithMany(p => p.UebDokumentZug)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("ueb_dokument_zug_kunde_ref_fkey");

            entity.HasOne(d => d.RegelwegAbBstRefNavigation).WithMany(p => p.UebDokumentZugRegelwegAbBstRefNavigation)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("ueb_dokument_zug_regelweg_ab_bst_ref_fkey");

            entity.HasOne(d => d.RegelwegZielBstRefNavigation).WithMany(p => p.UebDokumentZugRegelwegZielBstRefNavigation)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("ueb_dokument_zug_regelweg_ziel_bst_ref_fkey");

            entity.HasOne(d => d.UebDokumentRefNavigation).WithMany(p => p.UebDokumentZug).HasConstraintName("fk_ueb_zug_dok");
        });

        modelBuilder.Entity<UebDokumentZugKnotenzeiten>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("ueb_dokument_zug_knotenzeiten_pkey");

            entity.Property(e => e.Lfdnr).HasDefaultValue(1L);
            entity.Property(e => e.Relativlage).HasDefaultValue(0L);
            entity.Property(e => e.UebDokumentZugRef).HasDefaultValue(0L);
            entity.Property(e => e.UpdatedAt).HasDefaultValueSql("now()");

            entity.HasOne(d => d.BstRefNavigation).WithMany(p => p.UebDokumentZugKnotenzeiten).HasConstraintName("ueb_dokument_zug_fahrplan_bst_ref_fkey");

            entity.HasOne(d => d.UebDokumentZugRefNavigation).WithMany(p => p.UebDokumentZugKnotenzeiten).HasConstraintName("fk_ueb_kzeit_zug");
        });

        modelBuilder.Entity<UebDokumentZugRegelung>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("pk_ueb_zugreg");

            entity.Property(e => e.AnkerBstRef).HasDefaultValue(0L);
            entity.Property(e => e.UpdatedAt).HasDefaultValueSql("now()");

            entity.HasOne(d => d.AnkerBstRefNavigation).WithMany(p => p.UebDokumentZugRegelung)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("fk_ueb_zreg_bst");

            entity.HasOne(d => d.UebDokumentZugRefNavigation).WithMany(p => p.UebDokumentZugRegelung).HasConstraintName("fk_ueb_zreg_zug");
        });

        modelBuilder.Entity<UjbauSender>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("ujbau_sender_pkey");

            entity.Property(e => e.Id).HasDefaultValueSql("nextval('ujbaudb.zvf_sender_id_seq'::regclass)");
            entity.Property(e => e.UpdatedAt).HasDefaultValueSql("now()");
        });

        modelBuilder.Entity<UjbauVorgang>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("ujbau_vorgang_pkey");

            entity.Property(e => e.Id).HasDefaultValueSql("nextval('ujbaudb.zvf_vorgang_id_seq'::regclass)");
            entity.Property(e => e.IstKs).HasDefaultValue(false);
            entity.Property(e => e.IstQs).HasDefaultValue(false);
            entity.Property(e => e.Kategorie).HasDefaultValueSql("'F'::character varying");
            entity.Property(e => e.UpdatedAt).HasDefaultValueSql("now()");
        });

        modelBuilder.Entity<UjbauVorgangBbmn>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("ujbau_vorgang_bbmn_pkey");

            entity.Property(e => e.Id).HasDefaultValueSql("nextval('ujbaudb.zvf_vorgang_bbmn_id_seq'::regclass)");
            entity.Property(e => e.UpdatedAt).HasDefaultValueSql("now()");

            entity.HasOne(d => d.UjVorgangRefNavigation).WithMany(p => p.UjbauVorgangBbmn).HasConstraintName("ujbau_vorgang_bbmn_uj_vorgang_ref_fkey");
        });

        modelBuilder.Entity<ZvfDokument>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("zvf_dokument_pkey");

            entity.HasIndex(e => e.Dateiname, "idx_zvf_dokument_dateiname_imported").HasFilter("(import_timestamp IS NOT NULL)");

            entity.Property(e => e.Endstueck).HasDefaultValue(false);
            entity.Property(e => e.RegionRef).HasDefaultValue(0L);
            entity.Property(e => e.SenderRef).HasDefaultValue(0L);
            entity.Property(e => e.UpdatedAt).HasDefaultValueSql("now()");
            entity.Property(e => e.VersionMajor).HasDefaultValue(0L);
            entity.Property(e => e.VersionMinor).HasDefaultValue(0L);
            entity.Property(e => e.VersionSub).HasDefaultValue(0L);

            entity.HasOne(d => d.RegionRefNavigation).WithMany(p => p.ZvfDokument)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("zvf_dokument_region_ref_fkey");

            entity.HasOne(d => d.SenderRefNavigation).WithMany(p => p.ZvfDokument)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("zvf_dokument_sender_ref_fkey");

            entity.HasOne(d => d.UjbauVorgangRefNavigation).WithMany(p => p.ZvfDokument)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("zvf_dokument_ujbau_vorgang_ref_fkey");
        });

        modelBuilder.Entity<ZvfDokumentStreckenabschnitte>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("zvf_doc_strabs_pkey");

            entity.Property(e => e.UpdatedAt).HasDefaultValueSql("now()");
            entity.Property(e => e.ZeitraumUnterbrochen).HasDefaultValue(false);
            entity.Property(e => e.ZvfDokumentRef).HasDefaultValue(0L);

            entity.HasOne(d => d.ZvfDokumentRefNavigation).WithMany(p => p.ZvfDokumentStreckenabschnitte).HasConstraintName("zvf_dokument_streckenabschnitte_zvf_dokument_ref_fkey");
        });

        modelBuilder.Entity<ZvfDokumentZug>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("zvf_dokument_zug_pkey");

            entity.Property(e => e.Bedarf).HasDefaultValue(false);
            entity.Property(e => e.KundeRef).HasDefaultValue(0L);
            entity.Property(e => e.RegelwegAbgangBstRef).HasDefaultValue(0L);
            entity.Property(e => e.RegelwegZielBstRef).HasDefaultValue(0L);
            entity.Property(e => e.Sonderzug).HasDefaultValue(false);
            entity.Property(e => e.UpdateAt).HasDefaultValueSql("now()");

            entity.HasOne(d => d.KundeRefNavigation).WithMany(p => p.ZvfDokumentZug)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("zvf_dokument_zug_kunde_ref_fkey");

            entity.HasOne(d => d.RegelwegAbgangBstRefNavigation).WithMany(p => p.ZvfDokumentZugRegelwegAbgangBstRefNavigation).HasConstraintName("zvf_dokument_zug_regelweg_abgang_bst_ref_fkey");

            entity.HasOne(d => d.RegelwegZielBstRefNavigation).WithMany(p => p.ZvfDokumentZugRegelwegZielBstRefNavigation).HasConstraintName("zvf_dokument_zug_regelweg_ziel_bst_ref_fkey");

            entity.HasOne(d => d.ZvfDokumentRefNavigation).WithMany(p => p.ZvfDokumentZug).HasConstraintName("zvf_dokument_zug_zvf_dokument_ref_fkey");
        });

        modelBuilder.Entity<ZvfDokumentZugAbweichung>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("zvf_dokument_zug_abweichung_pkey");

            entity.HasIndex(e => e.Abweichung, "idx_zvfzugab_abweichung_json")
                .HasMethod("gin")
                .HasOperators(new[] { "jsonb_path_ops" });

            entity.Property(e => e.UpdatedAt).HasDefaultValueSql("now()");
            entity.Property(e => e.ZvfDokumentZugRef).HasDefaultValue(0L);

            entity.HasOne(d => d.AbBstRefNavigation).WithMany(p => p.ZvfDokumentZugAbweichung).HasConstraintName("fk_zvf_zugabw_bst_ref");

            entity.HasOne(d => d.ZvfDokumentZugRefNavigation).WithMany(p => p.ZvfDokumentZugAbweichung).HasConstraintName("zvf_dokument_zug_abweichung_zvf_dokument_zug_ref_fkey");
        });

        modelBuilder.Entity<ZvfDokumentZugEntfallen>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("zvf_dokument_zug_entfallen_pkey");

            entity.Property(e => e.UpdatedAt).HasDefaultValueSql("now()");

            entity.HasOne(d => d.ZvfDokumentRefNavigation).WithMany(p => p.ZvfDokumentZugEntfallen).HasConstraintName("zvf_dokument_zug_entfallen_zvf_dokument_ref_fkey");
        });

        OnModelCreatingPartial(modelBuilder);
    }

    partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
}
