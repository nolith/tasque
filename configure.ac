dnl Warning: This is an automatically generated file, do not edit!
dnl Process this file with autoconf to produce a configure script.
AC_PREREQ([2.54])
AC_INIT([tasque], [0.1.9])
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
PKG_CHECK_MODULES([GTK_SHARP_2_12], gtk-sharp-2.0 >= 2.12.0, GTK_2_12="yes", GTK_2_12="no")
AM_CONDITIONAL(GTK_2_12, test "$GTK_2_12" = "yes")
PKG_CHECK_MODULES([NDESK_DBUS_10], [ndesk-dbus-1.0])
PKG_CHECK_MODULES([NDESK_DBUS_GLIB_10], [ndesk-dbus-glib-1.0])

#
# Allow the project to build without notify-sharp
#
PKG_CHECK_MODULES(NOTIFY_SHARP, notify-sharp, enable_notify_sharp="yes", enable_notify_sharp="no")
AM_CONDITIONAL(ENABLE_NOTIFY_SHARP, test "x$enable_notify_sharp" != "xno")
AC_SUBST(NOTIFY_SHARP_LIBS)

AC_ARG_ENABLE(debug,
	AC_HELP_STRING([--enable-debug],
		[Use 'Debug' Configuration [default=yes]]),
		enable_debug=$enableval, enable_debug=no)
AM_CONDITIONAL(ENABLE_DEBUG, test "x$enable_debug" != "xno")
if test "x$enable_debug" != "xno" ; then
#	DEBUG_CONFIG_LIBRARIES=' ${pkglibdir}/Db4objects.Db4o.dll'
#	DEBUG_CONFIG_LIBS=' -r:${pkglibdir}/Db4objects.Db4o.dll'
#	AC_SUBST(DEBUG_CONFIG_LIBRARIES)
#	AC_SUBST(DEBUG_CONFIG_LIBS)
	CONFIG_REQUESTED="yes"
fi
AC_ARG_ENABLE(release,
	AC_HELP_STRING([--enable-release],
		[Use 'Release' Configuration [default=no]]),
		enable_release=$enableval, enable_release=no)
AM_CONDITIONAL(ENABLE_RELEASE, test "x$enable_release" != "xno")
if test "x$enable_release" != "xno" ; then
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


#CSC_DEFINES=""
#if pkg-config --atleast-version=2.12 gtk-sharp-2.0; then
#	CSC_DEFINES="-d:GTK_2_12"
#fi
#AC_SUBST(CSC_DEFINES)

# Set default values for final_*
# This doesn't affect anything, as final_* values
# are always overridden. Just helps when printing the
# values at the end
final_backend_dummy=no
final_backend_eds=no
final_backend_icecore=no
final_backend_rtm=no
final_backend_sqlite=no
final_backend_hiveminder=no
final_experimental_backends=no

#
# All Backends = standard + experimental.
#
AC_ARG_ENABLE(all_backends,
	AC_HELP_STRING([--enable-all-backends],
		[Enable all backends, including standard and experimental [default=no]]),
		final_all_backends=$enableval, final_all_backends=no)
AM_CONDITIONAL(ENABLE_ALL_BACKENDS, test "x$final_all_backends" != "xno")

if test "x$final_all_backends" != "xno" ; then
   final_standard_backends=yes
   final_experimental_backends=yes
fi

#
# Standard Backends
#
AC_ARG_ENABLE(standard_backends,
	AC_HELP_STRING([--enable-standard-backends],
		[Enable standard (RTM, SQLite, Evolution) backends. [default=yes]]),
		final_standard_backends=$enableval, final_standard_backend=yes)
AM_CONDITIONAL(ENABLE_STANDARD_BACKENDS, test "x$final_standard_backends" != "xno")

if test "x$final_standard_backends" != "xno" ; then
   final_backend_rtm=yes
   final_backend_sqlite=yes
   final_backend_eds=yes
fi

#
# Experimental Backends
#
AC_ARG_ENABLE(experimental_backends,
	AC_HELP_STRING([--enable-experimental-backends],
		[Enable experimental (Hiveminder, ICECore) backends. [default=no]]),
		final_experimental_backends=$enableval, )
AM_CONDITIONAL(ENABLE_EXPERIMENTAL_BACKENDS, test "x$final_experimental_backends" != "xno")

if test "x$final_experimental_backends" != "xno" ; then
   final_backend_hiveminder=yes
   final_backend_icecore=yes
fi

#
# Dummy Backend (for debugging)
#
AC_ARG_ENABLE(backend_dummy,
	AC_HELP_STRING([--enable-backend-dummy],
		[Enable the Dummy (Debug) Backend [default=no]]),
		final_backend_dummy=$enableval, final_backend_dummy=no)
AM_CONDITIONAL(ENABLE_BACKEND_DUMMY, test "x$final_backend_dummy" != "xno")

#
# Remember the Milk Backend
#
AC_ARG_ENABLE(backend_rtm,
	AC_HELP_STRING([--enable-backend-rtm],
		[Enable the RTM Backend [default=yes]]),
		final_backend_rtm=$enableval, )

#
# SQLite Backend
#
AC_ARG_ENABLE(backend_sqlite,
	AC_HELP_STRING([--enable-backend-sqlite],
		[Enable the SQLite Backend [default=no]]),
		final_backend_sqlite=$enableval, )
AM_CONDITIONAL(ENABLE_BACKEND_SQLITE, test "x$final_backend_sqlite" != "xno")

#
# ICEcore for IceBackend Support
#
AC_ARG_ENABLE(backend_icecore,
	AC_HELP_STRING([--enable-backend-icecore],
		[Enable the ICEcore Backend [default=no]]),
		final_backend_icecore=$enableval, )
AM_CONDITIONAL(ENABLE_BACKEND_ICECORE, test "x$final_backend_icecore" != "xno")
if test "x$final_backend_icecore" != "xno" ; then
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
               final_backend_eds=$enableval, )
AM_CONDITIONAL(ENABLE_BACKEND_EDS, test "x$final_backend_eds" != "xno")
if test "x$final_backend_eds" != "xno" ; then
       # FIXME : Is this the right way to do this ?
       PKG_CHECK_MODULES(EVOLUTION_SHARP, evolution-sharp >= 0.18.1)
fi
AC_SUBST(EVOLUTION_SHARP_LIBS)

#
# Hiveminder Backend
#
AC_ARG_ENABLE(backend_hiveminder,
	AC_HELP_STRING([--enable-backend-hiveminder],
		[Enable the Hiveminder Backend [default=no]]),
		final_backend_hiveminder=$enableval, )
AM_CONDITIONAL(ENABLE_BACKEND_HIVEMINDER, test "x$final_backend_hiveminder" != "xno")

#
# If no backends were enabled,
# enable the RTM Backend by default.
#
if test "x$final_backend_dummy" = "xno" -a "x$final_backend_rtm" = "xno" \
	-a "x$final_backend_sqlite" = "xno" -a "x$final_backend_icecore" = "xno" \
	-a "x$final_backend_eds" = "xno" -a "x$final_backend_hiveminder" = "xno"; then
	# No other backend was enabled, so enable
	# the RTM Backend by default
	final_backend_rtm=yes
fi
# Define ENABLE_BACKEND_RTM here so it only gets defined once
AM_CONDITIONAL(ENABLE_BACKEND_RTM, test "x$final_backend_rtm" != "xno")

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
	Dummy Backend:     ${final_backend_dummy}
	Evolution Backend: ${final_backend_eds}
	ICECore Backend:   ${final_backend_icecore}
	RTM Backend:       ${final_backend_rtm}
	SQLite Backend:    ${final_backend_sqlite}
	Hiveminder Backend: ${final_backend_hiveminder}
"

