using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using Dalamud.Interface;
using Dalamud.Interface.Colors;
using Dalamud.Interface.Components;
using Dalamud.Interface.Utility.Raii;
using ImGuiNET;
using MOAction.Configuration;
using Newtonsoft.Json;

namespace MOAction.Windows.Config;

public partial class ConfigWindow
{
    private int Wizard()
    {
        using var tabItem = ImRaii.TabItem("Wizard");
        if (!tabItem.Success)
            return 0;

        ImGui.TextUnformatted("This window has some beginner friendly stacks for you to import.");
        // TODO: don't hardcode import strings
        //TODO maybe use the files in the folder below, this is that json, compacted & BASE64 encoded
        if (ImGui.Button("Import Gapcloser basics"))
        {
            ImportStringToMouseOverActions("W3siQmFzZUlkIjoxNTUsIlN0YWNrIjpbeyJJdGVtMSI6IlVJIE1vdXNlb3ZlciIsIkl0ZW0yIjoxNTV9LHsiSXRlbTEiOiJGaWVsZCBNb3VzZW92ZXIiLCJJdGVtMiI6MTU1fSx7Ikl0ZW0xIjoiQ3Jvc3NoYWlyIiwiSXRlbTIiOjE1NX0seyJJdGVtMSI6IlRhcmdldCIsIkl0ZW0yIjoxNTV9XSwiTW9kaWZpZXIiOjAsIkpvYklkeCI6MjV9LHsiQmFzZUlkIjoyNTc2MiwiU3RhY2siOlt7Ikl0ZW0xIjoiVUkgTW91c2VvdmVyIiwiSXRlbTIiOjI1NzYyfSx7Ikl0ZW0xIjoiRmllbGQgTW91c2VvdmVyIiwiSXRlbTIiOjI1NzYyfSx7Ikl0ZW0xIjoiQ3Jvc3NoYWlyIiwiSXRlbTIiOjI1NzYyfSx7Ikl0ZW0xIjoiVGFyZ2V0IiwiSXRlbTIiOjI1NzYyfV0sIk1vZGlmaWVyIjowLCJKb2JJZHgiOjIwfSx7IkJhc2VJZCI6MjI2MiwiU3RhY2siOlt7Ikl0ZW0xIjoiVUkgTW91c2VvdmVyIiwiSXRlbTIiOjIyNjJ9LHsiSXRlbTEiOiJGaWVsZCBNb3VzZW92ZXIiLCJJdGVtMiI6MjI2Mn0seyJJdGVtMSI6IkNyb3NzaGFpciIsIkl0ZW0yIjoyMjYyfSx7Ikl0ZW0xIjoiVGFyZ2V0IiwiSXRlbTIiOjIyNjJ9XSwiTW9kaWZpZXIiOjAsIkpvYklkeCI6MzB9LHsiQmFzZUlkIjoyNDI5NSwiU3RhY2siOlt7Ikl0ZW0xIjoiVUkgTW91c2VvdmVyIiwiSXRlbTIiOjI0Mjk1fSx7Ikl0ZW0xIjoiRmllbGQgTW91c2VvdmVyIiwiSXRlbTIiOjI0Mjk1fSx7Ikl0ZW0xIjoiQ3Jvc3NoYWlyIiwiSXRlbTIiOjI0Mjk1fSx7Ikl0ZW0xIjoiVGFyZ2V0IiwiSXRlbTIiOjI0Mjk1fV0sIk1vZGlmaWVyIjowLCJKb2JJZHgiOjQwfSx7IkJhc2VJZCI6MzQ2NDYsIlN0YWNrIjpbeyJJdGVtMSI6IlVJIE1vdXNlb3ZlciIsIkl0ZW0yIjozNDY0Nn0seyJJdGVtMSI6IkZpZWxkIE1vdXNlb3ZlciIsIkl0ZW0yIjozNDY0Nn0seyJJdGVtMSI6IkNyb3NzaGFpciIsIkl0ZW0yIjozNDY0Nn0seyJJdGVtMSI6IlRhcmdldCIsIkl0ZW0yIjozNDY0Nn1dLCJNb2RpZmllciI6MCwiSm9iSWR4Ijo0MX1d");
            Plugin.SaveStacks();
        }
        if (ImGui.Button("Import Tank Basics"))
        {
            ImportStringToMouseOverActions("W3siQmFzZUlkIjo3NTM3LCJTdGFjayI6W3siSXRlbTEiOiI8Mj4iLCJJdGVtMiI6NzUzN30seyJJdGVtMSI6IlRhcmdldCIsIkl0ZW0yIjo3NTM3fV0sIk1vZGlmaWVyIjowLCJKb2JJZHgiOjMyfSx7IkJhc2VJZCI6NzM5MywiU3RhY2siOlt7Ikl0ZW0xIjoiVUkgTW91c2VvdmVyIiwiSXRlbTIiOjczOTN9LHsiSXRlbTEiOiJUYXJnZXQiLCJJdGVtMiI6NzM5M30seyJJdGVtMSI6IjwyPiIsIkl0ZW0yIjo3MzkzfV0sIk1vZGlmaWVyIjowLCJKb2JJZHgiOjMyfSx7IkJhc2VJZCI6MjU3NTQsIlN0YWNrIjpbeyJJdGVtMSI6IlVJIE1vdXNlb3ZlciIsIkl0ZW0yIjoyNTc1NH0seyJJdGVtMSI6IlRhcmdldCIsIkl0ZW0yIjoyNTc1NH1dLCJNb2RpZmllciI6MCwiSm9iSWR4IjozMn0seyJCYXNlSWQiOjc1MzcsIlN0YWNrIjpbeyJJdGVtMSI6IjwyPiIsIkl0ZW0yIjo3NTM3fSx7Ikl0ZW0xIjoiVGFyZ2V0IiwiSXRlbTIiOjc1Mzd9XSwiTW9kaWZpZXIiOjAsIkpvYklkeCI6MzJ9LHsiQmFzZUlkIjoyNTc1OCwiU3RhY2siOlt7Ikl0ZW0xIjoiVUkgTW91c2VvdmVyIiwiSXRlbTIiOjI1NzU4fSx7Ikl0ZW0xIjoiVGFyZ2V0IiwiSXRlbTIiOjI1NzU4fSx7Ikl0ZW0xIjoiPDI+IiwiSXRlbTIiOjI1NzU4fV0sIk1vZGlmaWVyIjowLCJKb2JJZHgiOjM3fSx7IkJhc2VJZCI6MTYxNTEsIlN0YWNrIjpbeyJJdGVtMSI6IlVJIE1vdXNlb3ZlciIsIkl0ZW0yIjoxNjE1MX0seyJJdGVtMSI6IlRhcmdldCIsIkl0ZW0yIjoxNjE1MX1dLCJNb2RpZmllciI6MCwiSm9iSWR4IjozN30seyJCYXNlSWQiOjc1MzcsIlN0YWNrIjpbeyJJdGVtMSI6IjwyPiIsIkl0ZW0yIjo3NTM3fSx7Ikl0ZW0xIjoiVGFyZ2V0IiwiSXRlbTIiOjc1Mzd9XSwiTW9kaWZpZXIiOjAsIkpvYklkeCI6MzJ9LHsiQmFzZUlkIjozNTQxLCJTdGFjayI6W3siSXRlbTEiOiJVSSBNb3VzZW92ZXIiLCJJdGVtMiI6MzU0MX0seyJJdGVtMSI6IlRhcmdldCIsIkl0ZW0yIjozNTQxfV0sIk1vZGlmaWVyIjowLCJKb2JJZHgiOjE5fSx7IkJhc2VJZCI6NzM4MiwiU3RhY2siOlt7Ikl0ZW0xIjoiVUkgTW91c2VvdmVyIiwiSXRlbTIiOjczODJ9LHsiSXRlbTEiOiJUYXJnZXQiLCJJdGVtMiI6NzM4Mn0seyJJdGVtMSI6IjwyPiIsIkl0ZW0yIjo3MzgyfV0sIk1vZGlmaWVyIjowLCJKb2JJZHgiOjE5fSx7IkJhc2VJZCI6MjcsIlN0YWNrIjpbeyJJdGVtMSI6IlVJIE1vdXNlb3ZlciIsIkl0ZW0yIjoyN30seyJJdGVtMSI6IlRhcmdldCIsIkl0ZW0yIjoyN31dLCJNb2RpZmllciI6MCwiSm9iSWR4IjoxOX0seyJCYXNlSWQiOjc1MzcsIlN0YWNrIjpbeyJJdGVtMSI6IjwyPiIsIkl0ZW0yIjo3NTM3fSx7Ikl0ZW0xIjoiVGFyZ2V0IiwiSXRlbTIiOjc1Mzd9XSwiTW9kaWZpZXIiOjAsIkpvYklkeCI6MzJ9LHsiQmFzZUlkIjoxNjQ2NCwiU3RhY2siOlt7Ikl0ZW0xIjoiVUkgTW91c2VvdmVyIiwiSXRlbTIiOjE2NDY0fSx7Ikl0ZW0xIjoiVGFyZ2V0IiwiSXRlbTIiOjE2NDY0fSx7Ikl0ZW0xIjoiPDI+IiwiSXRlbTIiOjE2NDY0fV0sIk1vZGlmaWVyIjowLCJKb2JJZHgiOjIxfV0=");
            Plugin.SaveStacks();
        }
        if (ImGui.Button("Import White Mage Basics"))
        {
            ImportStringToMouseOverActions("W3siQmFzZUlkIjoxMjAsIlN0YWNrIjpbeyJJdGVtMSI6IlVJIE1vdXNlb3ZlciIsIkl0ZW0yIjoxMjB9LHsiSXRlbTEiOiJUYXJnZXQiLCJJdGVtMiI6MTIwfV0sIk1vZGlmaWVyIjowLCJKb2JJZHgiOjI0fSx7IkJhc2VJZCI6MTI1LCJTdGFjayI6W3siSXRlbTEiOiJVSSBNb3VzZW92ZXIiLCJJdGVtMiI6MTI1fSx7Ikl0ZW0xIjoiVGFyZ2V0IiwiSXRlbTIiOjEyNX1dLCJNb2RpZmllciI6MCwiSm9iSWR4IjoyNH0seyJCYXNlSWQiOjEzNywiU3RhY2siOlt7Ikl0ZW0xIjoiVUkgTW91c2VvdmVyIiwiSXRlbTIiOjEzN30seyJJdGVtMSI6IlRhcmdldCIsIkl0ZW0yIjoxMzd9XSwiTW9kaWZpZXIiOjAsIkpvYklkeCI6MjR9LHsiQmFzZUlkIjoxMzUsIlN0YWNrIjpbeyJJdGVtMSI6IlVJIE1vdXNlb3ZlciIsIkl0ZW0yIjoxMzV9LHsiSXRlbTEiOiJUYXJnZXQiLCJJdGVtMiI6MTM1fV0sIk1vZGlmaWVyIjowLCJKb2JJZHgiOjI0fSx7IkJhc2VJZCI6MTMxLCJTdGFjayI6W3siSXRlbTEiOiJVSSBNb3VzZW92ZXIiLCJJdGVtMiI6MTMxfSx7Ikl0ZW0xIjoiVGFyZ2V0IiwiSXRlbTIiOjEzMX1dLCJNb2RpZmllciI6MCwiSm9iSWR4IjoyNH0seyJCYXNlSWQiOjI1ODYyLCJTdGFjayI6W3siSXRlbTEiOiJTZWxmIiwiSXRlbTIiOjI1ODYyfV0sIk1vZGlmaWVyIjowLCJKb2JJZHgiOjI0fSx7IkJhc2VJZCI6MzU2OSwiU3RhY2siOlt7Ikl0ZW0xIjoiVGFyZ2V0IiwiSXRlbTIiOjM1Njl9XSwiTW9kaWZpZXIiOjAsIkpvYklkeCI6MjR9LHsiQmFzZUlkIjozNTcwLCJTdGFjayI6W3siSXRlbTEiOiJVSSBNb3VzZW92ZXIiLCJJdGVtMiI6MzU3MH0seyJJdGVtMSI6IlRhcmdldCIsIkl0ZW0yIjozNTcwfV0sIk1vZGlmaWVyIjowLCJKb2JJZHgiOjI0fSx7IkJhc2VJZCI6MTY1MzEsIlN0YWNrIjpbeyJJdGVtMSI6IlVJIE1vdXNlb3ZlciIsIkl0ZW0yIjoxNjUzMX0seyJJdGVtMSI6IlRhcmdldCIsIkl0ZW0yIjoxNjUzMX1dLCJNb2RpZmllciI6MCwiSm9iSWR4IjoyNH0seyJCYXNlSWQiOjI1ODYxLCJTdGFjayI6W3siSXRlbTEiOiJVSSBNb3VzZW92ZXIiLCJJdGVtMiI6MjU4NjF9LHsiSXRlbTEiOiJUYXJnZXQiLCJJdGVtMiI6MjU4NjF9XSwiTW9kaWZpZXIiOjAsIkpvYklkeCI6MjR9LHsiQmFzZUlkIjoxNDAsIlN0YWNrIjpbeyJJdGVtMSI6IlVJIE1vdXNlb3ZlciIsIkl0ZW0yIjoxNDB9LHsiSXRlbTEiOiJUYXJnZXQiLCJJdGVtMiI6MTQwfV0sIk1vZGlmaWVyIjowLCJKb2JJZHgiOjI0fSx7IkJhc2VJZCI6NzQzMiwiU3RhY2siOlt7Ikl0ZW0xIjoiVUkgTW91c2VvdmVyIiwiSXRlbTIiOjc0MzJ9LHsiSXRlbTEiOiJUYXJnZXQiLCJJdGVtMiI6NzQzMn1dLCJNb2RpZmllciI6MCwiSm9iSWR4IjoyNH1d");
            Plugin.SaveStacks();
        }
        if (ImGui.Button("Import Scholar Basics"))
        {
            ImportStringToMouseOverActions("W3siQmFzZUlkIjoxOTAsIlN0YWNrIjpbeyJJdGVtMSI6IlVJIE1vdXNlb3ZlciIsIkl0ZW0yIjoxOTB9LHsiSXRlbTEiOiJUYXJnZXQiLCJJdGVtMiI6MTkwfV0sIk1vZGlmaWVyIjowLCJKb2JJZHgiOjI4fSx7IkJhc2VJZCI6MTczLCJTdGFjayI6W3siSXRlbTEiOiJVSSBNb3VzZW92ZXIiLCJJdGVtMiI6MTczfSx7Ikl0ZW0xIjoiVGFyZ2V0IiwiSXRlbTIiOjE3M31dLCJNb2RpZmllciI6MCwiSm9iSWR4IjoyOH0seyJCYXNlSWQiOjE4NSwiU3RhY2siOlt7Ikl0ZW0xIjoiVUkgTW91c2VvdmVyIiwiSXRlbTIiOjE4NX0seyJJdGVtMSI6IlRhcmdldCIsIkl0ZW0yIjoxODV9XSwiTW9kaWZpZXIiOjAsIkpvYklkeCI6Mjh9LHsiQmFzZUlkIjoxODksIlN0YWNrIjpbeyJJdGVtMSI6IlVJIE1vdXNlb3ZlciIsIkl0ZW0yIjoxODl9LHsiSXRlbTEiOiJUYXJnZXQiLCJJdGVtMiI6MTg5fV0sIk1vZGlmaWVyIjowLCJKb2JJZHgiOjI4fSx7IkJhc2VJZCI6MTg4LCJTdGFjayI6W3siSXRlbTEiOiJUYXJnZXQiLCJJdGVtMiI6MTg4fV0sIk1vZGlmaWVyIjowLCJKb2JJZHgiOjI4fSx7IkJhc2VJZCI6MzU4NSwiU3RhY2siOlt7Ikl0ZW0xIjoiVUkgTW91c2VvdmVyIiwiSXRlbTIiOjM1ODV9LHsiSXRlbTEiOiJUYXJnZXQiLCJJdGVtMiI6MzU4NX1dLCJNb2RpZmllciI6MCwiSm9iSWR4IjoyOH0seyJCYXNlSWQiOjc0MzQsIlN0YWNrIjpbeyJJdGVtMSI6IlVJIE1vdXNlb3ZlciIsIkl0ZW0yIjo3NDM0fSx7Ikl0ZW0xIjoiVGFyZ2V0IiwiSXRlbTIiOjc0MzR9XSwiTW9kaWZpZXIiOjAsIkpvYklkeCI6Mjh9LHsiQmFzZUlkIjo3NDM3LCJTdGFjayI6W3siSXRlbTEiOiJVSSBNb3VzZW92ZXIiLCJJdGVtMiI6NzQzN30seyJJdGVtMSI6IlRhcmdldCIsIkl0ZW0yIjo3NDM3fV0sIk1vZGlmaWVyIjowLCJKb2JJZHgiOjI4fSx7IkJhc2VJZCI6MjU4NjcsIlN0YWNrIjpbeyJJdGVtMSI6IlVJIE1vdXNlb3ZlciIsIkl0ZW0yIjoyNTg2N30seyJJdGVtMSI6IlRhcmdldCIsIkl0ZW0yIjoyNTg2N31dLCJNb2RpZmllciI6MCwiSm9iSWR4IjoyOH1d");
            Plugin.SaveStacks();
        }
        if (ImGui.Button("Import Astrologian Basics"))
        {
            ImportStringToMouseOverActions("W3siQmFzZUlkIjozNTk0LCJTdGFjayI6W3siSXRlbTEiOiJVSSBNb3VzZW92ZXIiLCJJdGVtMiI6MzU5NH0seyJJdGVtMSI6IlRhcmdldCIsIkl0ZW0yIjozNTk0fV0sIk1vZGlmaWVyIjowLCJKb2JJZHgiOjMzfSx7IkJhc2VJZCI6MzYwMywiU3RhY2siOlt7Ikl0ZW0xIjoiVUkgTW91c2VvdmVyIiwiSXRlbTIiOjM2MDN9LHsiSXRlbTEiOiJUYXJnZXQiLCJJdGVtMiI6MzYwM31dLCJNb2RpZmllciI6MCwiSm9iSWR4IjozM30seyJCYXNlSWQiOjM2MTAsIlN0YWNrIjpbeyJJdGVtMSI6IlVJIE1vdXNlb3ZlciIsIkl0ZW0yIjozNjEwfSx7Ikl0ZW0xIjoiVGFyZ2V0IiwiSXRlbTIiOjM2MTB9XSwiTW9kaWZpZXIiOjAsIkpvYklkeCI6MzN9LHsiQmFzZUlkIjozNTk1LCJTdGFjayI6W3siSXRlbTEiOiJVSSBNb3VzZW92ZXIiLCJJdGVtMiI6MzU5NX0seyJJdGVtMSI6IlRhcmdldCIsIkl0ZW0yIjozNTk1fV0sIk1vZGlmaWVyIjowLCJKb2JJZHgiOjMzfSx7IkJhc2VJZCI6MzYxNCwiU3RhY2siOlt7Ikl0ZW0xIjoiVUkgTW91c2VvdmVyIiwiSXRlbTIiOjM2MTR9LHsiSXRlbTEiOiJUYXJnZXQiLCJJdGVtMiI6MzYxNH1dLCJNb2RpZmllciI6MCwiSm9iSWR4IjozM30seyJCYXNlSWQiOjM3MDE5LCJTdGFjayI6W3siSXRlbTEiOiJVSSBNb3VzZW92ZXIiLCJJdGVtMiI6MzcwMTl9LHsiSXRlbTEiOiJUYXJnZXQiLCJJdGVtMiI6MzcwMTl9XSwiTW9kaWZpZXIiOjAsIkpvYklkeCI6MzN9LHsiQmFzZUlkIjozNzAyMCwiU3RhY2siOlt7Ikl0ZW0xIjoiVUkgTW91c2VvdmVyIiwiSXRlbTIiOjM3MDIwfSx7Ikl0ZW0xIjoiVGFyZ2V0IiwiSXRlbTIiOjM3MDIwfV0sIk1vZGlmaWVyIjowLCJKb2JJZHgiOjMzfSx7IkJhc2VJZCI6MzcwMjEsIlN0YWNrIjpbeyJJdGVtMSI6IlVJIE1vdXNlb3ZlciIsIkl0ZW0yIjozNzAyMX0seyJJdGVtMSI6IlRhcmdldCIsIkl0ZW0yIjozNzAyMX1dLCJNb2RpZmllciI6MCwiSm9iSWR4IjozM30seyJCYXNlSWQiOjM2MTIsIlN0YWNrIjpbeyJJdGVtMSI6IlVJIE1vdXNlb3ZlciIsIkl0ZW0yIjozNjEyfSx7Ikl0ZW0xIjoiVGFyZ2V0IiwiSXRlbTIiOjM2MTJ9XSwiTW9kaWZpZXIiOjAsIkpvYklkeCI6MzN9LHsiQmFzZUlkIjo3NDM5LCJTdGFjayI6W3siSXRlbTEiOiJUYXJnZXQiLCJJdGVtMiI6NzQzOX1dLCJNb2RpZmllciI6MCwiSm9iSWR4IjozM30seyJCYXNlSWQiOjE2NTU2LCJTdGFjayI6W3siSXRlbTEiOiJVSSBNb3VzZW92ZXIiLCJJdGVtMiI6MTY1NTZ9LHsiSXRlbTEiOiJUYXJnZXQiLCJJdGVtMiI6MTY1NTZ9XSwiTW9kaWZpZXIiOjAsIkpvYklkeCI6MzN9LHsiQmFzZUlkIjoyNTg3MywiU3RhY2siOlt7Ikl0ZW0xIjoiVUkgTW91c2VvdmVyIiwiSXRlbTIiOjI1ODczfSx7Ikl0ZW0xIjoiVGFyZ2V0IiwiSXRlbTIiOjI1ODczfV0sIk1vZGlmaWVyIjowLCJKb2JJZHgiOjMzfV0=");
            Plugin.SaveStacks();
        }
        if (ImGui.Button("Import Sage Basics"))
        {
            ImportStringToMouseOverActions("W3siQmFzZUlkIjoyNDI4NCwiU3RhY2siOlt7Ikl0ZW0xIjoiVUkgTW91c2VvdmVyIiwiSXRlbTIiOjI0Mjg0fSx7Ikl0ZW0xIjoiVGFyZ2V0IiwiSXRlbTIiOjI0Mjg0fV0sIk1vZGlmaWVyIjowLCJKb2JJZHgiOjQwfSx7IkJhc2VJZCI6MjQyODcsIlN0YWNrIjpbeyJJdGVtMSI6IlVJIE1vdXNlb3ZlciIsIkl0ZW0yIjoyNDI4N30seyJJdGVtMSI6IlRhcmdldCIsIkl0ZW0yIjoyNDI4N31dLCJNb2RpZmllciI6MCwiSm9iSWR4Ijo0MH0seyJCYXNlSWQiOjI0Mjg1LCJTdGFjayI6W3siSXRlbTEiOiJVSSBNb3VzZW92ZXIiLCJJdGVtMiI6MjQyODV9LHsiSXRlbTEiOiJUYXJnZXQiLCJJdGVtMiI6MjQyODV9XSwiTW9kaWZpZXIiOjAsIkpvYklkeCI6NDB9LHsiQmFzZUlkIjoyNDI5NiwiU3RhY2siOlt7Ikl0ZW0xIjoiVUkgTW91c2VvdmVyIiwiSXRlbTIiOjI0Mjk2fSx7Ikl0ZW0xIjoiVGFyZ2V0IiwiSXRlbTIiOjI0Mjk2fV0sIk1vZGlmaWVyIjowLCJKb2JJZHgiOjQwfSx7IkJhc2VJZCI6MjQzMDMsIlN0YWNrIjpbeyJJdGVtMSI6IlVJIE1vdXNlb3ZlciIsIkl0ZW0yIjoyNDMwM30seyJJdGVtMSI6IlRhcmdldCIsIkl0ZW0yIjoyNDMwM31dLCJNb2RpZmllciI6MCwiSm9iSWR4Ijo0MH0seyJCYXNlSWQiOjI0MzA1LCJTdGFjayI6W3siSXRlbTEiOiJVSSBNb3VzZW92ZXIiLCJJdGVtMiI6MjQzMDV9LHsiSXRlbTEiOiJUYXJnZXQiLCJJdGVtMiI6MjQzMDV9XSwiTW9kaWZpZXIiOjAsIkpvYklkeCI6NDB9LHsiQmFzZUlkIjoyNDMxNywiU3RhY2siOlt7Ikl0ZW0xIjoiVUkgTW91c2VvdmVyIiwiSXRlbTIiOjI0MzE3fSx7Ikl0ZW0xIjoiVGFyZ2V0IiwiSXRlbTIiOjI0MzE3fV0sIk1vZGlmaWVyIjowLCJKb2JJZHgiOjQwfV0=");
            Plugin.SaveStacks();
        }
        return 3;
    }

}
