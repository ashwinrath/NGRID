using System.Windows.Forms;

namespace NGRID.GUI
{
    /// <summary>
    /// This class is created to make easy common GUI tasks.
    /// </summary>
    public static class NGRIDGuiHelper
    {
        #region MessageBoxes

        /// <summary>
        /// Show a message box that show an error.
        /// </summary>
        /// <param name="message">Message to show</param>
        public static void ShowErrorMessage(string message)
        {
            ShowErrorMessage(message, "Error!");
        }

        /// <summary>
        /// Show a message box that show an error.
        /// </summary>
        /// <param name="message">Message to show</param>
        /// <param name="caption">Caption of message box</param>
        public static void ShowErrorMessage(string message, string caption)
        {
            MessageBox.Show(message, caption, MessageBoxButtons.OK, MessageBoxIcon.Error);
        }

        /// <summary>
        /// Show a message box that show an warning.
        /// </summary>
        /// <param name="message">Message to show</param>
        public static void ShowWarningMessage(string message)
        {
            ShowWarningMessage(message, "Warning!");
        }

        /// <summary>
        /// Show a message box that show an warning.
        /// </summary>
        /// <param name="message">Message to show</param>
        /// <param name="caption">Caption of message box</param>
        public static void ShowWarningMessage(string message, string caption)
        {
            MessageBox.Show(message, caption, MessageBoxButtons.OK, MessageBoxIcon.Warning);
        }

        /// <summary>
        /// Shows a messagebox to ask a question to user.
        /// </summary>
        /// <param name="message">Message to show</param>
        /// <param name="caption">Caption of message box</param>
        /// <returns>User's choice</returns>
        public static DialogResult ShowQuestionDialog(string message, string caption)
        {
            return MessageBox.Show(message, caption, MessageBoxButtons.YesNo, MessageBoxIcon.Question);
        }

        /// <summary>
        /// Shows a messagebox to ask a question to user.
        /// </summary>
        /// <param name="message">Message to show</param>
        /// <param name="caption">Caption of message box</param>
        /// <param name="defaultButton">Default selected button</param>
        /// <returns>User's choice</returns>
        public static DialogResult ShowQuestionDialog(string message, string caption, MessageBoxDefaultButton defaultButton)
        {
            return MessageBox.Show(message, caption, MessageBoxButtons.YesNo, MessageBoxIcon.Question, defaultButton);
        }

        /// <summary>
        /// Shows a messagebox that shows an information.
        /// </summary>
        /// <param name="message">Message to show</param>
        /// <param name="caption">Caption of message box</param>
        public static void ShowInfoDialog(string message, string caption)
        {
            MessageBox.Show(message, caption, MessageBoxButtons.OK, MessageBoxIcon.Information);
        }

        #endregion
    }
}
