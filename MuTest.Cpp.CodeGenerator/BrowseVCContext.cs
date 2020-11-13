using System;
using Microsoft.EntityFrameworkCore;
using MuTest.Cpp.CodeGenerator.Model;
using IOFile = System.IO.File;

#nullable disable

namespace MuTest.Cpp.CodeGenerator
{
    public class BrowseVCContext : DbContext
    {
        private readonly string _sqliteDbPath;

        public BrowseVCContext(string sqliteDbPath)
        {
            if (string.IsNullOrWhiteSpace(sqliteDbPath) || !IOFile.Exists(sqliteDbPath))
            {
                throw new ArgumentNullException(
                    nameof(sqliteDbPath),
                    "Sqlite Path cannot be null or whitespace or should exists");
            }

            _sqliteDbPath = sqliteDbPath;
        }

        public virtual DbSet<AssocSpan> AssocSpans { get; set; }
        public virtual DbSet<AssocText> AssocTexts { get; set; }
        public virtual DbSet<BaseClassParent> BaseClassParents { get; set; }
        public virtual DbSet<CodeItem> CodeItems { get; set; }
        public virtual DbSet<CodeItemKind> CodeItemKinds { get; set; }
        public virtual DbSet<Config> Configs { get; set; }
        public virtual DbSet<ConfigFile> ConfigFiles { get; set; }
        public virtual DbSet<File> Files { get; set; }
        public virtual DbSet<FileMap> FileMaps { get; set; }
        public virtual DbSet<FileSignature> FileSignatures { get; set; }
        public virtual DbSet<HintFile> HintFiles { get; set; }
        public virtual DbSet<Parser> Parsers { get; set; }
        public virtual DbSet<Project> Projects { get; set; }
        public virtual DbSet<ProjectRef> ProjectRefs { get; set; }
        public virtual DbSet<Property> Properties { get; set; }
        public virtual DbSet<SharedText> SharedTexts { get; set; }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            if (!optionsBuilder.IsConfigured)
            {
                optionsBuilder.UseSqlite($"Data Source={_sqliteDbPath}");
            }
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<AssocSpan>(entity =>
            {
                entity.HasNoKey();

                entity.ToTable("assoc_spans");

                entity.HasIndex(e => e.CodeItemId, "ix_assoc_spans_code_item_id");

                entity.HasIndex(e => new { e.CodeItemId, e.Kind }, "uq_assoc_spans_code_item_id_kind")
                    .IsUnique();

                entity.Property(e => e.CodeItemId)
                    .HasColumnType("bigint")
                    .HasColumnName("code_item_id");

                entity.Property(e => e.EndColumn)
                    .HasColumnType("integer")
                    .HasColumnName("end_column");

                entity.Property(e => e.EndLine)
                    .HasColumnType("integer")
                    .HasColumnName("end_line");

                entity.Property(e => e.Kind)
                    .HasColumnType("tinyint")
                    .HasColumnName("kind");

                entity.Property(e => e.StartColumn)
                    .HasColumnType("integer")
                    .HasColumnName("start_column");

                entity.Property(e => e.StartLine)
                    .HasColumnType("integer")
                    .HasColumnName("start_line");

                entity.HasOne(d => d.CodeItem)
                    .WithMany()
                    .HasForeignKey(d => d.CodeItemId);
            });

            modelBuilder.Entity<AssocText>(entity =>
            {
                entity.HasNoKey();

                entity.ToTable("assoc_text");

                entity.HasIndex(e => e.CodeItemId, "ix_assoc_text_code_item_id");

                entity.HasIndex(e => new { e.CodeItemId, e.Kind }, "uq_assoc_text_code_item_id_kind")
                    .IsUnique();

                entity.Property(e => e.CodeItemId)
                    .HasColumnType("bigint")
                    .HasColumnName("code_item_id");

                entity.Property(e => e.Kind)
                    .HasColumnType("tinyint")
                    .HasColumnName("kind");

                entity.Property(e => e.Text)
                    .IsRequired()
                    .HasColumnType("text")
                    .HasColumnName("text");

                entity.HasOne(d => d.CodeItem)
                    .WithMany()
                    .HasForeignKey(d => d.CodeItemId);
            });

            modelBuilder.Entity<BaseClassParent>(entity =>
            {
                entity.HasNoKey();

                entity.ToTable("base_class_parents");

                entity.HasIndex(e => e.BaseCodeItemId, "ix_base_class_parents_base_code_item_id");

                entity.HasIndex(e => e.ParentCodeItemId, "ix_base_class_parents_parent_code_item_id");

                entity.HasIndex(e => new { e.BaseCodeItemId, e.ParentCodeItemId }, "uq_base_class_parents_base_code_item_id_parent_code_item_id")
                    .IsUnique();

                entity.Property(e => e.BaseCodeItemId)
                    .HasColumnType("bigint")
                    .HasColumnName("base_code_item_id");

                entity.Property(e => e.ParentCodeItemId)
                    .HasColumnType("bigint")
                    .HasColumnName("parent_code_item_id");

                entity.HasOne(d => d.BaseCodeItem)
                    .WithMany()
                    .HasForeignKey(d => d.BaseCodeItemId)
                    .OnDelete(DeleteBehavior.ClientSetNull);

                entity.HasOne(d => d.ParentCodeItem)
                    .WithMany()
                    .HasForeignKey(d => d.ParentCodeItemId)
                    .OnDelete(DeleteBehavior.ClientSetNull);
            });

            modelBuilder.Entity<CodeItem>(entity =>
            {
                entity.ToTable("code_items");

                entity.HasIndex(e => e.FileId, "ix_code_items_file_id");

                entity.HasIndex(e => e.LowerNameHint, "ix_code_items_lower_name_hint");

                entity.HasIndex(e => e.Name, "ix_code_items_name");

                entity.HasIndex(e => e.ParentId, "ix_code_items_parent_id");

                entity.HasIndex(e => new { e.ParentId, e.Kind }, "ix_code_items_parent_id_kind");

                entity.Property(e => e.Id)
                    .HasColumnType("bigint")
                    .ValueGeneratedNever()
                    .HasColumnName("id");

                entity.Property(e => e.Attributes)
                    .HasColumnType("integer")
                    .HasColumnName("attributes");

                entity.Property(e => e.EndColumn)
                    .HasColumnType("integer")
                    .HasColumnName("end_column");

                entity.Property(e => e.EndLine)
                    .HasColumnType("integer")
                    .HasColumnName("end_line");

                entity.Property(e => e.FileId)
                    .HasColumnType("bigint")
                    .HasColumnName("file_id");

                entity.Property(e => e.Kind)
                    .HasColumnType("integer")
                    .HasColumnName("kind");

                entity.Property(e => e.LowerNameHint)
                    .IsRequired()
                    .HasColumnType("text")
                    .HasColumnName("lower_name_hint");

                entity.Property(e => e.Name)
                    .IsRequired()
                    .HasColumnType("text")
                    .HasColumnName("name");

                entity.Property(e => e.NameEndColumn)
                    .HasColumnType("integer")
                    .HasColumnName("name_end_column");

                entity.Property(e => e.NameEndLine)
                    .HasColumnType("integer")
                    .HasColumnName("name_end_line");

                entity.Property(e => e.NameStartColumn)
                    .HasColumnType("integer")
                    .HasColumnName("name_start_column");

                entity.Property(e => e.NameStartLine)
                    .HasColumnType("integer")
                    .HasColumnName("name_start_line");

                entity.Property(e => e.ParamDefaultValue)
                    .HasColumnType("text")
                    .HasColumnName("param_default_value");

                entity.Property(e => e.ParamDefaultValueEndColumn)
                    .HasColumnType("integer")
                    .HasColumnName("param_default_value_end_column");

                entity.Property(e => e.ParamDefaultValueEndLine)
                    .HasColumnType("integer")
                    .HasColumnName("param_default_value_end_line");

                entity.Property(e => e.ParamDefaultValueStartColumn)
                    .HasColumnType("integer")
                    .HasColumnName("param_default_value_start_column");

                entity.Property(e => e.ParamDefaultValueStartLine)
                    .HasColumnType("integer")
                    .HasColumnName("param_default_value_start_line");

                entity.Property(e => e.ParamNumber)
                    .HasColumnType("smallint")
                    .HasColumnName("param_number");

                entity.Property(e => e.ParentId)
                    .HasColumnType("bigint")
                    .HasColumnName("parent_id");

                entity.Property(e => e.StartColumn)
                    .HasColumnType("integer")
                    .HasColumnName("start_column");

                entity.Property(e => e.StartLine)
                    .HasColumnType("integer")
                    .HasColumnName("start_line");

                entity.Property(e => e.Type)
                    .HasColumnType("text")
                    .HasColumnName("type");
            });

            modelBuilder.Entity<CodeItemKind>(entity =>
            {
                entity.ToTable("code_item_kinds");

                entity.HasIndex(e => new { e.Name, e.ParserGuid }, "uq_code_item_kinds_name_parser_guid")
                    .IsUnique();

                entity.Property(e => e.Id)
                    .HasColumnType("integer")
                    .ValueGeneratedNever()
                    .HasColumnName("id");

                entity.Property(e => e.Name)
                    .IsRequired()
                    .HasColumnType("text")
                    .HasColumnName("name");

                entity.Property(e => e.ParserGuid)
                    .IsRequired()
                    .HasColumnType("text")
                    .HasColumnName("parser_guid");

                entity.HasOne(d => d.ParserGu)
                    .WithMany(p => p.CodeItemKinds)
                    .HasForeignKey(d => d.ParserGuid)
                    .OnDelete(DeleteBehavior.ClientSetNull);
            });

            modelBuilder.Entity<Config>(entity =>
            {
                entity.ToTable("configs");

                entity.HasIndex(e => e.Name, "ix_configs_name");

                entity.HasIndex(e => e.ProjectId, "ix_configs_project_id");

                entity.HasIndex(e => new { e.ProjectId, e.Name }, "uq_configs_project_id_name")
                    .IsUnique();

                entity.Property(e => e.Id)
                    .HasColumnType("bigint")
                    .ValueGeneratedNever()
                    .HasColumnName("id");

                entity.Property(e => e.ConfigFrameworkIncludePath)
                    .HasColumnType("bigint")
                    .HasColumnName("config_framework_include_path");

                entity.Property(e => e.ConfigIncludePath)
                    .HasColumnType("bigint")
                    .HasColumnName("config_include_path");

                entity.Property(e => e.ConfigOptions)
                    .HasColumnType("bigint")
                    .HasColumnName("config_options");

                entity.Property(e => e.ExcludePath)
                    .HasColumnType("bigint")
                    .HasColumnName("exclude_path");

                entity.Property(e => e.Hash)
                    .HasColumnType("bigint")
                    .HasColumnName("hash");

                entity.Property(e => e.Name)
                    .IsRequired()
                    .HasColumnType("text")
                    .HasColumnName("name");

                entity.Property(e => e.PlatformFrameworkIncludePath)
                    .HasColumnType("bigint")
                    .HasColumnName("platform_framework_include_path");

                entity.Property(e => e.PlatformIncludePath)
                    .HasColumnType("bigint")
                    .HasColumnName("platform_include_path");

                entity.Property(e => e.PlatformOptions)
                    .HasColumnType("bigint")
                    .HasColumnName("platform_options");

                entity.Property(e => e.ProjectId)
                    .HasColumnType("bigint")
                    .HasColumnName("project_id");

                entity.Property(e => e.ToolsetIsenseIdentifier)
                    .HasColumnType("text")
                    .HasColumnName("toolset_isense_identifier");

                entity.HasOne(d => d.Project)
                    .WithMany(p => p.Configs)
                    .HasForeignKey(d => d.ProjectId);
            });

            modelBuilder.Entity<ConfigFile>(entity =>
            {
                entity.HasNoKey();

                entity.ToTable("config_files");

                entity.HasIndex(e => e.ConfigId, "ix_config_files_config_id");

                entity.HasIndex(e => e.FileId, "ix_config_files_file_id");

                entity.HasIndex(e => new { e.ConfigId, e.FileId }, "uq_config_files_config_id_file_id")
                    .IsUnique();

                entity.Property(e => e.Compiled)
                    .HasColumnType("tinyint")
                    .HasColumnName("compiled");

                entity.Property(e => e.CompiledPch)
                    .HasColumnType("tinyint")
                    .HasColumnName("compiled_pch");

                entity.Property(e => e.ConfigFinal)
                    .HasColumnType("tinyint")
                    .HasColumnName("config_final");

                entity.Property(e => e.ConfigId)
                    .HasColumnType("bigint")
                    .HasColumnName("config_id");

                entity.Property(e => e.Explicit)
                    .HasColumnType("tinyint")
                    .HasColumnName("explicit");

                entity.Property(e => e.FileId)
                    .HasColumnType("bigint")
                    .HasColumnName("file_id");

                entity.Property(e => e.FrameworkIncludePath)
                    .HasColumnType("bigint")
                    .HasColumnName("framework_include_path");

                entity.Property(e => e.Generated)
                    .HasColumnType("tinyint")
                    .HasColumnName("generated");

                entity.Property(e => e.Implicit)
                    .HasColumnType("tinyint")
                    .HasColumnName("implicit");

                entity.Property(e => e.IncludePath)
                    .HasColumnType("bigint")
                    .HasColumnName("include_path");

                entity.Property(e => e.Options)
                    .HasColumnType("bigint")
                    .HasColumnName("options");

                entity.Property(e => e.Reference)
                    .HasColumnType("tinyint")
                    .HasColumnName("reference");

                entity.Property(e => e.Shared)
                    .HasColumnType("tinyint")
                    .HasColumnName("shared");

                entity.HasOne(d => d.Config)
                    .WithMany()
                    .HasForeignKey(d => d.ConfigId);

                entity.HasOne(d => d.File)
                    .WithMany()
                    .HasForeignKey(d => d.FileId);
            });

            modelBuilder.Entity<File>(entity =>
            {
                entity.ToTable("files");

                entity.HasIndex(e => e.LeafName, "ix_files_leaf_name");

                entity.HasIndex(e => e.Name, "uq_files_name")
                    .IsUnique();

                entity.Property(e => e.Id)
                    .HasColumnType("bigint")
                    .ValueGeneratedNever()
                    .HasColumnName("id");

                entity.Property(e => e.Addtime)
                    .HasColumnType("integer")
                    .HasColumnName("addtime");

                entity.Property(e => e.Attributes)
                    .HasColumnType("integer")
                    .HasColumnName("attributes");

                entity.Property(e => e.Difftime)
                    .HasColumnType("integer")
                    .HasColumnName("difftime");

                entity.Property(e => e.LeafName)
                    .IsRequired()
                    .HasColumnType("text")
                    .HasColumnName("leaf_name");

                entity.Property(e => e.Name)
                    .IsRequired()
                    .HasColumnType("text")
                    .HasColumnName("name");

                entity.Property(e => e.ParserGuid)
                    .IsRequired()
                    .HasColumnType("text")
                    .HasColumnName("parser_guid");

                entity.Property(e => e.Parsetime)
                    .HasColumnType("integer")
                    .HasColumnName("parsetime");

                entity.Property(e => e.Timestamp)
                    .HasColumnType("bigint")
                    .HasColumnName("timestamp");

                entity.HasOne(d => d.ParserGu)
                    .WithMany(p => p.Files)
                    .HasForeignKey(d => d.ParserGuid)
                    .OnDelete(DeleteBehavior.ClientSetNull);
            });

            modelBuilder.Entity<FileMap>(entity =>
            {
                entity.HasNoKey();

                entity.ToTable("file_map");

                entity.HasIndex(e => e.CodeItemId, "ix_file_map_code_item_id");

                entity.HasIndex(e => e.ConfigId, "ix_file_map_config_id");

                entity.HasIndex(e => e.FileId, "ix_file_map_file_id");

                entity.HasIndex(e => new { e.CodeItemId, e.ConfigId, e.FileId }, "uq_file_map_code_item_id_config_id_file_id")
                    .IsUnique();

                entity.Property(e => e.CodeItemId)
                    .HasColumnType("bigint")
                    .HasColumnName("code_item_id");

                entity.Property(e => e.ConfigId)
                    .HasColumnType("bigint")
                    .HasColumnName("config_id");

                entity.Property(e => e.FileId)
                    .HasColumnType("bigint")
                    .HasColumnName("file_id");
            });

            modelBuilder.Entity<FileSignature>(entity =>
            {
                entity.HasNoKey();

                entity.ToTable("file_signatures");

                entity.HasIndex(e => e.FileId, "ix_file_signatures_file_id");

                entity.HasIndex(e => new { e.FileId, e.Kind }, "uq_file_signatures_file_id_kind")
                    .IsUnique();

                entity.Property(e => e.FileId)
                    .HasColumnType("bigint")
                    .HasColumnName("file_id");

                entity.Property(e => e.Kind)
                    .HasColumnType("tinyint")
                    .HasColumnName("kind");

                entity.Property(e => e.Signature)
                    .IsRequired()
                    .HasColumnType("blob")
                    .HasColumnName("signature");

                entity.HasOne(d => d.File)
                    .WithMany()
                    .HasForeignKey(d => d.FileId);
            });

            modelBuilder.Entity<HintFile>(entity =>
            {
                entity.ToTable("hint_files");

                entity.HasIndex(e => e.Name, "uq_hint_files_name")
                    .IsUnique();

                entity.Property(e => e.Id)
                    .HasColumnType("bigint")
                    .ValueGeneratedNever()
                    .HasColumnName("id");

                entity.Property(e => e.Name)
                    .IsRequired()
                    .HasColumnType("text")
                    .HasColumnName("name");

                entity.Property(e => e.Timestamp)
                    .HasColumnType("bigint")
                    .HasColumnName("timestamp");
            });

            modelBuilder.Entity<Parser>(entity =>
            {
                entity.HasKey(e => e.ParserGuid);

                entity.ToTable("parsers");

                entity.Property(e => e.ParserGuid)
                    .HasColumnType("text")
                    .HasColumnName("parser_guid");

                entity.Property(e => e.Name)
                    .IsRequired()
                    .HasColumnType("text")
                    .HasColumnName("name");

                entity.Property(e => e.ShortName)
                    .IsRequired()
                    .HasColumnType("text")
                    .HasColumnName("short_name");
            });

            modelBuilder.Entity<Project>(entity =>
            {
                entity.ToTable("projects");

                entity.HasIndex(e => e.Guid, "uq_projects_guid")
                    .IsUnique();

                entity.HasIndex(e => e.Name, "uq_projects_name")
                    .IsUnique();

                entity.Property(e => e.Id)
                    .HasColumnType("bigint")
                    .ValueGeneratedNever()
                    .HasColumnName("id");

                entity.Property(e => e.Guid)
                    .IsRequired()
                    .HasColumnType("text")
                    .HasColumnName("guid");

                entity.Property(e => e.Name)
                    .IsRequired()
                    .HasColumnType("text")
                    .HasColumnName("name");

                entity.Property(e => e.Shared)
                    .HasColumnType("tinyint")
                    .HasColumnName("shared");
            });

            modelBuilder.Entity<ProjectRef>(entity =>
            {
                entity.HasNoKey();

                entity.ToTable("project_refs");

                entity.HasIndex(e => e.ConfigId, "ix_project_refs_config_id");

                entity.HasIndex(e => e.ProjectRefGuid, "ix_project_refs_project_ref_guid");

                entity.HasIndex(e => new { e.ConfigId, e.ResolvedName }, "uq_project_refs_config_id_resolved_name")
                    .IsUnique();

                entity.Property(e => e.ConfigId)
                    .HasColumnType("bigint")
                    .HasColumnName("config_id");

                entity.Property(e => e.ProjectRefGuid)
                    .HasColumnType("bigint")
                    .HasColumnName("project_ref_guid");

                entity.Property(e => e.ProjectRefName)
                    .HasColumnType("bigint")
                    .HasColumnName("project_ref_name");

                entity.Property(e => e.ResolvedName)
                    .HasColumnType("bigint")
                    .HasColumnName("resolved_name");

                entity.HasOne(d => d.Config)
                    .WithMany()
                    .HasForeignKey(d => d.ConfigId);
            });

            modelBuilder.Entity<Property>(entity =>
            {
                entity.HasKey(e => e.Name);

                entity.ToTable("properties");

                entity.Property(e => e.Name)
                    .HasColumnType("text")
                    .HasColumnName("name");

                entity.Property(e => e.Value)
                    .IsRequired()
                    .HasColumnType("text")
                    .HasColumnName("value");
            });

            modelBuilder.Entity<SharedText>(entity =>
            {
                entity.ToTable("shared_text");

                entity.HasIndex(e => e.Hash, "ix_shared_text_hash");

                entity.Property(e => e.Id)
                    .HasColumnType("bigint")
                    .ValueGeneratedNever()
                    .HasColumnName("id");

                entity.Property(e => e.Hash)
                    .HasColumnType("bigint")
                    .HasColumnName("hash");

                entity.Property(e => e.Text)
                    .IsRequired()
                    .HasColumnType("text")
                    .HasColumnName("text");
            });
        }
    }
}
