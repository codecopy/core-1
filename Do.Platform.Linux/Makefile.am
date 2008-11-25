EXTRA_DIST = 

include $(top_srcdir)/Makefile.include

ASSEMBLY_COMPILER_COMMAND = $(MCS)
ASSEMBLY_COMPILER_FLAGS = $(MCS_LINQ_FLAG) -noconfig -codepage:utf8 -warn:4
ASSEMBLY = Do.Platform.Linux.dll
ASSEMBLY_MDB = $(ASSEMBLY).mdb
COMPILE_TARGET = library

if ENABLE_DEBUG
ASSEMBLY_COMPILER_FLAGS += -debug -d:DEBUG
endif

all: $(ASSEMBLY)

FILES = \
	src/AssemblyInfo.cs \
	src/Do.Platform/ConsoleCrayon.cs \
	src/Do.Platform/ConsoleLogImplementation.cs \
	src/Do.Platform/EnvironmentImplementation.cs \
	src/Do.Platform/GConfPreferencesImplementation.cs \
	src/Do.Platform/PathsImplementation.cs \
	src/Do.Platform/UniverseFactoryImplementation.cs \
	src/Do.Universe/ApplicationItem.cs \
	src/Do.Universe/FileItem.cs \
	src/Plugins/Applications/ApplicationItemSource.cs \
	src/Plugins/Clipboard/CopyToClipboardAction.cs \
	src/Plugins/GNOME/GNOMESpecialLocationsItemSource.cs

RESOURCES =  \
        Resources/Do.Platform.Linux.addin.xml

REFERENCES =  \
	System \
	System.Core \
	Mono.Posix \
	$(GTK_SHARP_20_LIBS) \
	$(GNOME_SHARP_20_LIBS) \
	$(GNOME_DESKTOP_SHARP_20_LIBS) \
	$(GCONF_SHARP_20_LIBS) \
	../Do.Platform/Do.Platform.dll

$(ASSEMBLY) $(ASSEMBLY_MDB): $(build_sources) $(build_resources) $(build_datafiles) $(DLL_REFERENCES) $(PROJECT_REFERENCES) $(build_xamlg_list)
	$(ASSEMBLY_COMPILER_COMMAND) $(ASSEMBLY_COMPILER_FLAGS) -out:$(ASSEMBLY) -target:$(COMPILE_TARGET) $(build_sources_embed) $(build_resources_embed) $(build_references_ref)