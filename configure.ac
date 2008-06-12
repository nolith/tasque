dnl Warning: This is an automatically generated file, do not edit!
dnl Process this file with autoconf to produce a configure script.
AC_PREREQ([2.54])
AC_INIT([tasque], [0.1.6])
AM_INIT_AUTOMAKE([foreign tar-ustar])
AM_MAINTAINER_MODE

GNOME_COMMON_INIT

AC_PROG_INSTALL
IT_PROG_INTLTOOL([0.35])
AC_PROG_LIBTOOL

dnl pkg-config
PKG_PROG_PKG_CONFIG

AC_PROG_INSTALL

AC_PATH_PROG(GMCS, gmcs, no)
if test "x$GMCS" = "xno"; then
        AC_MSG_ERROR([gmcs Not found])
fi

#
# Setup GETTEXT
#

GETTEXT_PACKAGE=tasque
AC_SUBST(GETTEXT_PACKAGE)
AC_DEFINE_UNQUOTED(GETTEXT_PACKAGE, "$GETTEXT_PACKAGE", [Gettext package])
AM_GLIB_GNU_GETTEXT

#
# Find gconftool-2
#
AC_PATH_PROG(GCONFTOOL, gconftool-2)
AM_GCONF_SOURCE_2

PKG_CHECK_MODULES([GLIB_SHARP_20], [glib-sharp-2.0])
PKG_CHECK_MODULES([GNOME_SHARP_20], [gnome-sharp-2.0])
PKG_CHECK_MODULES([GTK_DOTNET_20], [gtk-dotnet-2.0])
PKG_CHECK_MODULES([GTK_SHARP_20], [gtk-sharp-2.0 >= 2.10.0])
PKG_CHECK_MODULES([NDESK_DBUS_10], [ndesk-dbus-1.0])
PKG_CHECK_MODULES([NDESK_DBUS_GLIB_10], [ndesk-dbus-glib-1.0])

#
# Allow the project to build without notify-sharp
#
PKG_CHECK_MODULES(NOTIFY_SHARP, notify-sharp, enable_notify_sharp="yes", enable_notify_sharp="no")
AM_CONDITIONAL(ENABLE_NOTIFY_SHARP, test "x$enable_notify_sharp" = "xyes")
AC_SUBST(NOTIFY_SHARP_LIBS)

AC_ARG_ENABLE(debug,
	AC_HELP_STRING([--enable-debug],
		[Use 'Debug' Configuration [default=yes]]),
		enable_debug=yes, enable_debug=no)
AM_CONDITIONAL(ENABLE_DEBUG, test "x$enable_debug" = "xyes")
if test "x$enable_debug" = "xyes" ; then
#	DEBUG_CONFIG_LIBRARIES=' ${pkglibdir}/Db4objects.Db4o.dll'
#	DEBUG_CONFIG_LIBS=' -r:${pkglibdir}/Db4objects.Db4o.dll'
#	AC_SUBST(DEBUG_CONFIG_LIBRARIES)
#	AC_SUBST(DEBUG_CONFIG_LIBS)
	CONFIG_REQUESTED="yes"
fi
AC_ARG_ENABLE(release,
	AC_HELP_STRING([--enable-release],
		[Use 'Release' Configuration [default=no]]),
		enable_release=yes, enable_release=no)
AM_CONDITIONAL(ENABLE_RELEASE, test "x$enable_release" = "xyes")
if test "x$enable_release" = "xyes" ; then
#	RELEASE_CONFIG_LIBRARIES=' ${pkglibdir}/Db4objects.Db4o.dll'
#	RELEASE_CONFIG_LIBS=' -r:${pkglibdir}/Db4objects.Db4o.dll'
#	AC_SUBST(RELEASE_CONFIG_LIBRARIES)
#	AC_SUBST(RELEASE_CONFIG_LIBS)
	CONFIG_REQUESTED="yes"
fi
if test -z "$CONFIG_REQUESTED" ; then
#	DEBUG_CONFIG_LIBRARIES=' ${pkglibdir}/Db4objects.Db4o.dll'
#	DEBUG_CONFIG_LIBS=' -r:${pkglibdir}/Db4objects.Db4o.dll'
#	AC_SUBST(DEBUG_CONFIG_LIBRARIES)
#	AC_SUBST(DEBUG_CONFIG_LIBS)
	AM_CONDITIONAL(ENABLE_DEBUG, true)
fi


#
# DEFINES uses for #if statements
#
AC_ARG_WITH(dbus_service_dir, [  --with-dbus-service-dir=DIR            Where to install Tasque's DBus service file.])
AM_CONDITIONAL(WITH_DBUS_SERVICE_DIR, test "x$with_dbus_service_dir" != "x")
if test "x$with_dbus_service_dir" != "x"; then
	DBUS_SERVICE_DIR=$with_dbus_service_dir
else
	DBUS_SERVICE_DIR=${datadir}/dbus-1/services
fi
AC_SUBST(DBUS_SERVICE_DIR)

#
# Dummy Backend (for debugging)
#
AC_ARG_ENABLE(backend_dummy,
	AC_HELP_STRING([--enable-backend-dummy],
		[Enable the Dummy (Debug) Backend [default=no]]),
		enable_backend_dummy=yes, enable_backend_dummy=no)
AM_CONDITIONAL(ENABLE_BACKEND_DUMMY, test "x$enable_backend_dummy" = "xyes")

#
# Remember the Milk Backend
#
AC_ARG_ENABLE(backend_rtm,
	AC_HELP_STRING([--enable-backend-rtm],
		[Enable the RTM Backend [default=yes]]),
		enable_backend_rtm=yes, enable_backend_rtm=no)

#
# SQLite Backend
#
AC_ARG_ENABLE(backend_sqlite,
	AC_HELP_STRING([--enable-backend-sqlite],
		[Enable the SQLite Backend [default=no]]),
		enable_backend_sqlite=yes, enable_backend_sqlite=no)
AM_CONDITIONAL(ENABLE_BACKEND_SQLITE, test "x$enable_backend_sqlite" = "xyes")

#
# ICEcore for IceBackend Support
#
AC_ARG_ENABLE(backend_icecore,
	AC_HELP_STRING([--enable-backend-icecore],
		[Enable the ICEcore Backend [default=no]]),
		enable_backend_icecore=yes, enable_backend_icecore=no)
AM_CONDITIONAL(ENABLE_BACKEND_ICECORE, test "x$enable_backend_icecore" = "xyes")
if test "x$enable_backend_icecore" = "xyes" ; then
       # FIXME : Is this the right way to do this ?
       PKG_CHECK_MODULES(ICE_DESKTOP, Novell.IceDesktop)
fi
AC_SUBST(ICE_DESKTOP_LIBS)

#
# Evolution-Sharp for EDSBackend Support
#

AC_ARG_ENABLE(backend_eds,
       AC_HELP_STRING([--enable-backend-eds],
               [Enable the EDS Backend [default=no]]),
               enable_backend_eds=yes, enable_backend_eds=no)
AM_CONDITIONAL(ENABLE_BACKEND_EDS, test "x$enable_backend_eds" = "xyes")
if test "x$enable_backend_eds" = "xyes" ; then
       # FIXME : Is this the right way to do this ?
       PKG_CHECK_MODULES(EVOLUTION_SHARP, evolution-sharp)
fi
AC_SUBST(EVOLUTION_SHARP_LIBS)

#
# If no backends were enabled,
# enable the RTM Backend by default.
#
if test "x$enable_backend_dummy" = "xno" ; then
	if test "x$enable_backend_rtm" = "xno" ; then
		if test "x$enable_backend_sqlite" = "xno" ; then
			if test "x$enable_backend_icecore" = "xno" ; then
				if test "x$enable_backend_eds" = "xno" ; then
					# No other backend was enabled, so enable
					# the RTM Backend by default
					enable_backend_rtm=yes
				fi
			fi
		fi
	fi
fi
# Define ENABLE_BACKEND_RTM here so it only gets defined once
AM_CONDITIONAL(ENABLE_BACKEND_RTM, test "x$enable_backend_rtm" = "xyes")

### Begin GAC tool ###

AC_PATH_PROG(GACUTIL, gacutil, no)
if test "x$GACUTIL" = "xno" ; then
        AC_MSG_ERROR([No gacutil tool found])
fi

AC_SUBST(GACUTIL)
GACUTIL_FLAGS='/gacdir $(DESTDIR)$(prefix)'
AC_SUBST(GACUTIL_FLAGS)

### End GAC tool ###

AC_CONFIG_FILES([
./Makefile
./RtmNet/Makefile
./src/Makefile
./src/tasque.pc
./src/Backends/Makefile
./src/Backends/IceCore/Makefile
./data/Makefile
./data/images/Makefile
./data/sounds/Makefile
./po/Makefile.in
])

#./src/Backends/Dummy/Makefile
#./src/Backends/EDS/Makefile
#./src/Backends/Rtm/Makefile
#./src/Backends/Sqlite/Makefile

AC_OUTPUT

echo "
Configuration:

	Prefix:            ${prefix}
	Debug build:       ${enable_debug}
	Release build:     ${enable_release}
	Notification:      ${enable_notify_sharp}
	Dummy Backend:     ${enable_backend_dummy}
	Evolution Backend: ${enable_backend_eds}
	ICECore Backend:   ${enable_backend_icecore}
	RTM Backend:       ${enable_backend_rtm}
	SQLite Backend:    ${enable_backend_sqlite}
"

