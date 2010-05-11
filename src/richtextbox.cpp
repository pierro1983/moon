/* -*- Mode: C++; tab-width: 8; indent-tabs-mode: t; c-basic-offset: 8 -*- */
/*
 * richtextbox.cpp: 
 *
 * Contact:
 *   Moonlight List (moonlight-list@lists.ximian.com)
 *
 * Copyright 2010 Novell, Inc. (http://www.novell.com)
 *
 * See the LICENSE file included with the distribution for details.
 */

#include <config.h>

#include <cairo.h>

#include "richtextbox.h"


//
// TextPointer
//

TextPointer::TextPointer ()
{
	SetObjectType (Type::TEXTPOINTER);
}


//
// TextSelection
//

TextSelection::TextSelection ()
{
	SetObjectType (Type::TEXTSELECTION);
}


//
// RichTextBox
//

RichTextArea::RichTextArea ()
{
	SetObjectType (Type::RICHTEXTAREA);
}