CSC = gmcs

TARGET = RtmNet.dll

if ENABLE_DEBUG
CSFLAGS =  -t:library -noconfig -codepage:utf8 -warn:4 -debug -d:DEBUG
endif
if ENABLE_RELEASE
CSFLAGS =  -t:library -noconfig -codepage:utf8 -warn:4
endif

CSFILES = \
	$(srcdir)/ApiKeyRequiredException.cs		\
	$(srcdir)/AssemblyInfo.cs			\
	$(srcdir)/Auth.cs				\
	$(srcdir)/AuthenticationRequiredException.cs	\
	$(srcdir)/Categories.cs				\
	$(srcdir)/Contacts.cs				\
	$(srcdir)/DateGranularity.cs			\
	$(srcdir)/Enums.cs				\
	$(srcdir)/Groups.cs				\
	$(srcdir)/GroupSearchResults.cs			\
	$(srcdir)/List.cs				\
	$(srcdir)/Methods.cs				\
	$(srcdir)/Note.cs				\
	$(srcdir)/Response.cs				\
	$(srcdir)/ResponseXmlException.cs		\
	$(srcdir)/RtmApiException.cs			\
	$(srcdir)/Rtm.cs				\
	$(srcdir)/RtmException.cs			\
	$(srcdir)/RtmWebException.cs			\
	$(srcdir)/SignatureRequiredException.cs		\
	$(srcdir)/Tags.cs				\
	$(srcdir)/Task.cs				\
	$(srcdir)/User.cs				\
	$(srcdir)/Utils.cs


RESOURCES = 

ASSEMBLIES =  \
	-r:System \
	-r:System.Web \
	-r:System.Xml

$(TARGET): $(CSFILES)
	$(CSC) -unsafe -out:$@ $(CSFLAGS) $^ $(ASSEMBLIES) $(RESOURCES)

tasquelibdir = $(prefix)/lib/tasque
tasquelib_DATA = $(TARGET)	

bin_SCRIPTS = $(WRAPPER)


EXTRA_DIST = \
	$(CSFILES)

CLEANFILES = \
	$(TARGET)					\
	$(TARGET).mdb

DISTCLEANFILES =                        \
	$(TARGET)			\
	$(TARGET).mdb
