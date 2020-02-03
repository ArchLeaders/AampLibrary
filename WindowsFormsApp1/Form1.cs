using System;
using System.Windows.Forms;
using AampLibraryCSharp;
using Syroot.Maths;
using System.Drawing;

namespace WindowsFormsApp1
{
    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();
            treeView1.ImageList = imgList;

            Bitmap bmp = new Bitmap(32, 32);
            Graphics g = Graphics.FromImage(bmp);
            g.Clear(treeView1.BackColor);

            imgList.Images.Add(bmp);
        }

        AampFile file;
        private void openToolStripMenuItem_Click(object sender, EventArgs e)
        {
            var ofd = new OpenFileDialog();
            if (ofd.ShowDialog() != DialogResult.OK)
                return;

             file = AampFile.LoadFile(ofd.FileName);

            treeView1.Nodes.Clear();
            treeView1.Nodes.Add(System.IO.Path.GetFileName(ofd.FileName));
            treeView1.Nodes[0].Nodes.Add($"Version: {file.Version}");
            treeView1.Nodes[0].Nodes.Add($"EffectName: {file.ParameterIOType}");
            treeView1.Nodes[0].Nodes.Add($"EffectType: {file.EffectType}");

            GetChildNodes(file.RootNode, treeView1.Nodes[0]);
        }

        void GetChildNodes(ParamList node, TreeNode parentNode)
        {
            string name = node.HashString;

            TreeNode newNode = new TreeNode(name);
            parentNode.Nodes.Add(newNode);

            if (node.childParams.Length > 0)
                newNode.Nodes.Add("list", "Lists {}");
            if (node.paramObjects.Length > 0)
                newNode.Nodes.Add("obj", "Objects {}");

            foreach (var param in node.childParams)
                GetChildNodes(param, newNode.Nodes["list"]);
            
            foreach (var paramObj in node.paramObjects)
                SetObjNode(paramObj, newNode.Nodes["obj"]);
        }

        ImageList imgList = new ImageList();

        void SetObjNode(ParamObject paramObj, TreeNode parentNode)
        {
            string name = paramObj.HashString;

            var objNode = new TreeNode(name);
            parentNode.Nodes.Add(objNode);

            foreach (var entry in paramObj.paramEntries)
            {
                var entryNode = new EditableEntry($"{entry.HashString}");
                entryNode.ImageIndex = 0;

                switch (entry.ParamType)
                {
                    case ParamType.Boolean:
                    case ParamType.Float: 
                    case ParamType.Int:
                    case ParamType.Uint:
                    case ParamType.String64: 
                    case ParamType.String32: 
                    case ParamType.String256: 
                    case ParamType.StringRef:
                        entryNode.Text = $"{entry.HashString} {entry.Value}";
                        break;
                    case ParamType.Vector2F:
                        var vec2 = (Vector2F)entry.Value;
                        entryNode.Text = $"{entry.HashString} {vec2.X} {vec2.Y}";
                        break;
                    case ParamType.Vector3F:
                        var vec3 = (Vector3F)entry.Value;
                        entryNode.Text = $"{entry.HashString} {vec3.X} {vec3.Y} {vec3.Z}";
                        break;
                    case ParamType.Vector4F:
                        var vec4 = (Vector4F)entry.Value;
                        entryNode.Text = $"{entry.HashString} {vec4.X} {vec4.Y} {vec4.Z} {vec4.W}";
                        break;
                    case ParamType.Color4F:
                        var col = (Vector4F)entry.Value;
                        entryNode.Text = $"{entry.HashString} {col.X} {col.Y} {col.Z} {col.W}";

                        int ImageIndex = imgList.Images.Count;
                        entryNode.ImageIndex = ImageIndex;

                        var color = System.Drawing.Color.FromArgb(
                        EditBox.FloatToIntClamp(col.W),
                        EditBox.FloatToIntClamp(col.X),
                        EditBox.FloatToIntClamp(col.Y),
                        EditBox.FloatToIntClamp(col.Z));

                        Bitmap bmp = new Bitmap(32, 32);
                        Graphics g = Graphics.FromImage(bmp);
                        g.Clear(color);

                        imgList.Images.Add(bmp);
                        break;
                    default:
                        break;
                }

                entryNode.entry = entry;
                objNode.Nodes.Add(entryNode);
            }
        }

        public class EditableEntry : TreeNode
        {
            public ParamEntry entry;
            public EditableEntry(string name)
            {
                Text = name;

                ContextMenu = new ContextMenu();
                ContextMenu.MenuItems.Add(new MenuItem("Edit", OpenEditor));
            }


            private void OpenEditor(object sender, EventArgs e)
            {
                EditBox editor = new EditBox();
                editor.LoadEntry(entry);

                if (editor.ShowDialog() == DialogResult.OK)
                {
                    editor.SaveEntry();

                    switch (entry.ParamType)
                    {
                        case ParamType.Boolean:
                        case ParamType.Float:
                        case ParamType.Int:
                        case ParamType.Uint:
                        case ParamType.String64:
                        case ParamType.String32:
                        case ParamType.String256:
                        case ParamType.StringRef:
                            Text = $"{entry.HashString} {entry.Value}";
                            break;
                        case ParamType.Vector2F:
                            var vec2 = (Vector2F)entry.Value;
                            Text = $"{entry.HashString} {vec2.X} {vec2.Y}";
                            break;
                        case ParamType.Vector3F:
                            var vec3 = (Vector3F)entry.Value;
                            Text = $"{entry.HashString} {vec3.X} {vec3.Y} {vec3.Z}";
                            break;
                        case ParamType.Vector4F:
                            var vec4 = (Vector4F)entry.Value;
                            Text = $"{entry.HashString} {vec4.X} {vec4.Y} {vec4.Z} {vec4.W}";
                            break;
                        case ParamType.Color4F:
                            var col = (Vector4F)entry.Value;
                            Text = $"{entry.HashString} {col.X} {col.Y} {col.Z} {col.W}";
                            break;
                    }
                }
            }
        }

        private void treeView1_AfterSelect(object sender, TreeViewEventArgs e)
        {
            var node = treeView1.SelectedNode;

       
        }


        private void saveToolStripMenuItem_Click(object sender, EventArgs e)
        {
            SaveFileDialog sfd = new SaveFileDialog();

            if (sfd.ShowDialog() == DialogResult.OK)
            {
                file.Save(sfd.FileName);
            }
        }
    }
}
