#!/usr/bin/env python
import sys, dbus, dbus.glib

#import pdb

# Get the D-Bus Session Bus
bus = dbus.SessionBus()

#Access the ICEcore Daemon Object
obj = bus.get_object("org.gnome.Tasque",
	"/org/gnome/Tasque/RemoteControl")

#Access the remote control interface
tasque = dbus.Interface(obj, "org.gnome.Tasque.RemoteControl")

for n in tasque.GetCategoryNames():
	print n

taskId = tasque.CreateTask ("Tasque", "Create a task via DBus", True)

print "taskId: " + taskId
