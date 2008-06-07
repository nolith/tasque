
pixmapdir = $(datadir)/pixmaps
pixmap_DATA = 	tasque-16.png \
		tasque-22.png \
		tasque-24.png \
		tasque-32.png \
		tasque-48.png \
		tasque.svg \
		note-16.png \
		rtmLogo.png

hicolordir = $(datadir)/icons/hicolor

tango_icons =	tasque-16.png \
		tasque-22.png \
		tasque-24.png \
		tasque-32.png \
		tasque-48.png \
		tasque.svg \
		note-16.png \
		rtmLogo.png

clock_icons = clock-16-0.png \
		clock-16-1.png \
		clock-16-2.png \
		clock-16-3.png \
		clock-16-4.png \
		clock-16-5.png \
		clock-16-6.png \
		clock-16-7.png \
		clock-16-8.png \
		clock-16-9.png \
		clock-16-10.png \
		clock-16-11.png 

install-data-local:
	@-$(mkinstalldirs) $(DESTDIR)$(hicolordir)/16x16/apps $(DESTDIR)$(hicolordir)/22x22/apps $(DESTDIR)$(hicolordir)/24x24/apps $(DESTDIR)$(hicolordir)/32x32/apps $(DESTDIR)$(hicolordir)/48x48/apps $(DESTDIR)$(hicolordir)/scalable/apps
	$(INSTALL_DATA) $(srcdir)/tasque-16.png $(DESTDIR)$(hicolordir)/16x16/apps/tasque.png
	$(INSTALL_DATA) $(srcdir)/tasque-22.png $(DESTDIR)$(hicolordir)/22x22/apps/tasque.png
	$(INSTALL_DATA) $(srcdir)/tasque-24.png $(DESTDIR)$(hicolordir)/24x24/apps/tasque.png
	$(INSTALL_DATA) $(srcdir)/tasque-32.png $(DESTDIR)$(hicolordir)/32x32/apps/tasque.png
	$(INSTALL_DATA) $(srcdir)/tasque-48.png $(DESTDIR)$(hicolordir)/48x48/apps/tasque.png
	$(INSTALL_DATA) $(srcdir)/tasque.svg $(DESTDIR)$(hicolordir)/scalable/apps/tasque.svg

gtk_update_icon_cache = gtk-update-icon-cache -f -t $(datadir)/icons/hicolor

install-data-hook: update-icon-cache
uninstall-hook: update-icon-cache
update-icon-cache:
	@-if test -z "$(DESTDIR)"; then \
		echo "Updating Gtk icon cache."; \
		$(gtk_update_icon_cache); \
	else \
		echo "*** Icon cache not updated.  After install, run this:"; \
		echo "***   $(gtk_update_icon_cache)"; \
	fi

uninstall-hook:
	rm -f $(DESTDIR)$(hicolordir)/16x16/apps/tasque.png
	rm -f $(DESTDIR)$(hicolordir)/22x22/apps/tasque.png
	rm -f $(DESTDIR)$(hicolordir)/24x24/apps/tasque.png
	rm -f $(DESTDIR)$(hicolordir)/32x32/apps/tasque.png
	rm -f $(DESTDIR)$(hicolordir)/48x48/apps/tasque.png
	rm -f $(DESTDIR)$(hicolordir)/scalable/apps/tasque.svg

noinst_DATA =

EXTRA_DIST = $(noinst_DATA) $(pixmap_DATA) $(tango_icons) $(clock_icons)
