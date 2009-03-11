/* -*- Mode: C++; tab-width: 8; indent-tabs-mode: t; c-basic-offset: 8 -*- */
/*
 * popup.cpp
 *
 * Copyright 2009 Novell, Inc. (http://www.novell.com)
 *
 * See the LICENSE file included with the distribution for details.
 * 
 */

#include "popup.h"
#include "runtime.h"

Popup::Popup ()
{
	SetObjectType (Type::POPUP);
	surface = NULL;
}

void
Popup::OnPropertyChanged (PropertyChangedEventArgs *args, MoonError *error)
{
	if (args->GetId () == Popup::IsOpenProperty) {
		Emit (Popup::IsOpenChangedEvent);
	}
	DependencyObject::OnPropertyChanged (args, error);
}

void
Popup::SetActiveSurface (Surface *surface)
{
	if (this->surface == surface)
		return;

	UIElement *child = GetChild ();
	if (this->surface && child) {
		printf ("\nClearing child");
		UIElement *topLevel = surface->GetToplevel ();
		topLevel->ElementRemoved (child);
	}
	
	this->surface = surface;
	
	if (this->surface && child) {
		printf ("\nAdding child to surface");
		UIElement *topLevel = surface->GetToplevel ();
		if (topLevel)
			printf ("\nToplevel is a: %s", topLevel->GetType ()->GetName ());	

		topLevel->ElementAdded (child);
	}
	SetSurface (surface);
}
