/*
 * moon-plugin.h: MoonLight browser plugin.
 *
 * Author:
 *   Everaldo Canuto (everaldo@novell.com)
 *
 * Copyright 2007 Novell, Inc. (http://www.novell.com)
 *
 * See the LICENSE file included with the distribution for details.
 * 
 */

#ifndef MOON_PLUGIN
#define MOON_PLUGIN

#include "moonlight.h"
#include "plugin-class.h"

class PluginInstance
{
 private:
  	uint16 mode;           // NP_EMBED, NP_FULL, or NP_BACKGROUND
	NPWindow *window;      // Mozilla window object
	NPP instance;          // Mozilla instance object
	NPObject* rootobject;  // Mozilla jscript object wrapper
	bool xembed_supported; // XEmbed Extension supported

	// Property fields
	bool isloaded;
	char *source;

	// Private methods
	void CreateWindow ();

 public:	
	PluginInstance (NPP instance, uint16 mode);
	~PluginInstance ();
	void Initialize (int argc, char* const argn[], char* const argv[]);
	void Finalize ();

	// Mozilla plugin related methods
	NPError GetValue (NPPVariable variable, void *result);
	NPError SetValue (NPNVariable variable, void *value);
	NPError SetWindow (NPWindow* window);
	NPError NewStream (NPMIMEType type, NPStream* stream, NPBool seekable, uint16* stype);
	NPError DestroyStream (NPStream* stream, NPError reason);
	void StreamAsFile (NPStream* stream, const char* fname);
	int32 WriteReady (NPStream* stream);
	int32 Write (NPStream* stream, int32 offset, int32 len, void* buffer);
	void UrlNotify (const char* url, NPReason reason, void* notifyData);
	void Print (NPPrint* platformPrint);
	int16 EventHandle (void* event);

	// Gtk controls
	GtkWidget *container;  // plugin container object
	Canvas *canvas;        // plugin canvas object
 	Surface *surface;      // plugin surface object

	// Property getters and setters
	bool getIsLoaded () { return this->isloaded; };
	char *getSource () { return this->source; }
 };

#endif /* MOON_PLUGIN */
