include $(top_srcdir)/Makefile.include

CSFLAGS = 		\
	-debug 		\
	-define:DEBUG 	\
	-target:library

ASSMS = \
	-r:$(top_builddir)/src/Tasque.exe \
	$(GLIB_SHARP_20_LIBS) \
	$(GNOME_SHARP_20_LIBS) \
	$(GTK_DOTNET_20_LIBS) \
	$(ICE_DESKTOP_LIBS) \
	$(NDESK_DBUS_10_LIBS) \
	$(NDESK_DBUS_GLIB_10_LIBS) \
	-r:System \
	-r:Mono.Posix
	-r:System.xml

BACKEND_NAME = IceCore.Backend
CSFILES = \
	IceBackend.cs \
	IceCategory.cs \
	IceNote.cs \
	IceTask.cs

if ENABLE_BACKEND_ICECORE
TARGET = $(BACKEND_NAME).dll
RESOURCES =

$(TARGET): $(CSFILES) $(top_builddir)/src/Tasque.exe
	$(CSC) -out:$@ $(CSFLAGS) $(ASSMS) $(CSFILES) $(RESOURCES)
endif

backendsdir = $(pkglibdir)
backends_DATA = 		\
	$(TARGET)

EXTRA_DIST =            	\
	$(CSFILES)

CLEANFILES =				\
	$(TARGET).mdb \
	$(TARGET)

