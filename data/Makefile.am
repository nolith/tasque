
SUBDIRS = images sounds 

@INTLTOOL_DESKTOP_RULE@

desktop_in_files = tasque.desktop.in
desktop_files    = $(desktop_in_files:.desktop.in=.desktop)

desktopdir   = $(datadir)/applications
desktop_DATA = $(desktop_files)

dbusservicedir       = $(DBUS_SERVICE_DIR)
dbusservice_in_files = org.gnome.Tasque.service.in
dbusservice_DATA     = $(dbusservice_in_files:.service.in=.service)

$(dbusservice_DATA): $(dbusservice_in_files) Makefile
	sed -e "s|\@bindir\@|$(bindir)|g"	\
	    -e "s|\@wrapper\@|tasque|g"		\
	    < $< > $@

noinst_DATA = 

EXTRA_DIST = 					\
	$(desktop_in_files)     		\
	$(noinst_DATA)				\
	org.gnome.Tasque.service.in

DISTCLEANFILES = 				\
	$(desktop_files)			\
	org.gnome.Tasque.service
	
