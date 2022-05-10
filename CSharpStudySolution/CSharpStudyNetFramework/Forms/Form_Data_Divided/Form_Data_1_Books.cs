﻿using CSharpStudyNetFramework.Extra;
using CSharpStudyNetFramework.Helpers;
using CSharpStudyNetFramework.ORM.Models;
using MetroFramework.Controls;
using System;
using System.Collections.Generic;
using System.Windows.Forms;

// ######################################################################
// Форма данных - Вкладка "Книги"
// ######################################################################

namespace CSharpStudyNetFramework.Forms.Form_Data_Divided
{
    /// <summary>Форма с данными</summary>
    public partial class Form_Data : Form_Base
    {
        /// <summary>Событие изменения выбранной строчки в таблице во вкладке "Книги"</summary>
        private void Grid_Books_SelectionChanged(object sender, EventArgs e)
        {
            ExceptionHelper.CheckCode(this, false, () => {
                // Если выбрана строчка - заменяем текст в текстбоксах на значения из выбранной записи
                if (this.Grid_Books.SelectedRows.Count > 0) {
                    // Кнопки изменения и удаления делаем доступными
                    this.Button_Books_Edit.Enabled = this.Button_Books_Delete.Enabled = true;

                    // Находим сущность, выбранную в таблице
                    int selected_id = Convert.ToInt32(this.Grid_Books.SelectedRows[0].Cells[0].Value);
                    Book selected_book = DatabaseHelper.SelectFirstOrFormException(
                        DatabaseHelper.db.Books,
                        selected_id
                    );

                    // -----------------------------------------
                    // Заполняем форму значениями сущности
                    // -----------------------------------------
                    int selected_author_index_in_combobox = -1;
                    if (selected_book.Author != null) {
                        selected_author_index_in_combobox = this.ComboBox_Books_Author.Items.IndexOf(selected_book.Author.ToString());
                    }
                    this.ComboBox_Books_Author.SelectedIndex = selected_author_index_in_combobox;

                    this.TextBox_Books_Title.Text = selected_book.Title;

                    int selected_group_index_in_combobox = -1;
                    if (selected_book.Group != null) {
                        selected_group_index_in_combobox = this.ComboBox_Books_Group.Items.IndexOf(selected_book.Group.ToString());
                    }
                    this.ComboBox_Books_Group.SelectedIndex = selected_group_index_in_combobox;

                    int selected_bookmaker_index_in_combobox = -1;
                    if (selected_book.Bookmaker != null) {
                        selected_bookmaker_index_in_combobox = this.ComboBox_Books_Bookmaker.Items.IndexOf(selected_book.Bookmaker.ToString());
                    }
                    this.ComboBox_Books_Bookmaker.SelectedIndex = selected_bookmaker_index_in_combobox;

                    this.NumericUpDown_Books_Year.Value = selected_book.PublicationYear;
                    this.DateTime_Books_CopyBooksDate.Value = selected_book.RegistrationDate;

                    // Обновляем изображение
                    this.PictureBox_Books_Cover.Image = ImageHelper.ByteArrayToImage(selected_book.Photo);
                    // -----------------------------------------
                }
                // Если нет выбора - очищаем текстбоксы
                else {
                    // Кнопки изменения и удаления делаем недоступными
                    this.Button_Books_Edit.Enabled = this.Button_Books_Delete.Enabled = false;

                    // Очищаем форму (значения по умолчанию)
                    this.ComboBox_Books_Author.SelectedIndex = -1;
                    this.TextBox_Books_Title.Clear();
                    this.ComboBox_Books_Group.SelectedIndex = -1;
                    this.ComboBox_Books_Bookmaker.SelectedIndex = -1;
                    this.NumericUpDown_Books_Year.Value = 2000;
                    this.DateTime_Books_CopyBooksDate.Value = DateTime.Now;
                    this.PictureBox_Books_Cover.Image = null;
                }
            });
        }

        /// <summary>Событие нажатия на кнопку "Загрузить изображение обложки"</summary>
        private void Button_Books_AddCover_Click(object sender, EventArgs e)
        {
            ExceptionHelper.CheckCode(this, false, () => {
                if (this.OpenFileDialog_Book.ShowDialog() == DialogResult.OK) {
                    this.PictureBox_Books_Cover.Load(this.OpenFileDialog_Book.FileName);
                }
            });
        }

        /// <summary>Событие нажатия на кнопку "Добавить книгу"</summary>
        private void Button_Books_Create_Click(object sender, EventArgs e)
        {
            ExceptionHelper.CheckCode(this, true, () => {
                this.CreateOrEditBook(true);
                this.UnfocusAll();

                // Выбираем последнюю строчку в таблице
                this.Grid_Books.ClearSelection();
                this.Grid_Books.Rows[this.Grid_Books.Rows.Count - 1].Selected = true;

                FormHelper.SendSuccessMessage(this, "Книга успешно добавлена!");
            });
        }

        /// <summary>Событие нажатия на кнопку "Изменить информацию о книге"</summary>
        private void Button_Books_Edit_Click(object sender, EventArgs e)
        {
            ExceptionHelper.CheckCode(this, true, () => {
                this.CreateOrEditBook(false);
                this.UnfocusAll();
                FormHelper.SendSuccessMessage(this, "Книга успешно отредактирована!");
            });
        }

        /// <summary>Создаёт новую или изменяет выбранную книгу (смотрит на поля ввода)</summary>
        /// <param name="is_new">Если true - из введённых данных будет создана новая книга. Если false - будет изменена выбранная книга в таблице</param>
        private void CreateOrEditBook(bool is_new)
        {
            // Автор книги
            Author selected_author = DatabaseHelper.SelectFirstOrFormException(
                DatabaseHelper.db.Authors,
                this.ComboBox_Books_Author.Text
            );
            // Название книги
            string title = this.TextBox_Books_Title.Text.Trim();
            if (title == "") {
                throw new FormException("Необходимо указать название книги!");
            }
            // Жанр книги
            Group selected_group = DatabaseHelper.SelectFirstOrFormException(
                DatabaseHelper.db.Groups,
                this.ComboBox_Books_Group.Text
            );
            // Издатель книги
            Bookmaker selected_bookmaker = DatabaseHelper.SelectFirstOrFormException(
                DatabaseHelper.db.Bookmakers,
                this.ComboBox_Books_Bookmaker.Text
            );

            // Создание новой книги
            if (is_new) {
                // Создаём и сохраняем новую книгу
                Book new_entity = new Book {
                    Author = selected_author,
                    Title = title,
                    Group = selected_group,
                    Bookmaker = selected_bookmaker,
                    PublicationYear = Convert.ToInt32(this.NumericUpDown_Books_Year.Value),
                    RegistrationDate = this.DateTime_Books_CopyBooksDate.Value,
                    Photo = ImageHelper.ImageToByteArray(this.PictureBox_Books_Cover.Image)
                };
                DatabaseHelper.db.Books.Add(new_entity);
            }
            // Изменение выбранной книги
            else {
                if (this.Grid_Books.SelectedRows.Count > 0) {
                    // Изменяем и сохраняем выбранного автора в таблице
                    int selected_id = Convert.ToInt32(this.Grid_Books.SelectedRows[0].Cells[0].Value);
                    Book found_entity = DatabaseHelper.SelectFirstOrFormException(
                        DatabaseHelper.db.Books,
                        selected_id
                    );

                    // Обновление полей
                    found_entity.Author = selected_author;
                    found_entity.Title = title;
                    found_entity.Group = selected_group;
                    found_entity.Bookmaker = selected_bookmaker;
                    found_entity.PublicationYear = Convert.ToInt32(this.NumericUpDown_Books_Year.Value);
                    found_entity.RegistrationDate = this.DateTime_Books_CopyBooksDate.Value;
                    found_entity.Photo = ImageHelper.ImageToByteArray(this.PictureBox_Books_Cover.Image);

                    DatabaseHelper.db.Books.Update(found_entity);
                } else {
                    throw new FormException("Книга для изменения не выбрана!");
                }
            }

            DatabaseHelper.db.SaveChanges();
            this.UpdateCurrentSelectedTab();
        }

        /// <summary>Событие нажатия на кнопку "Удалить книгу"</summary>
        private void Button_Books_Delete_Click(object sender, EventArgs e)
        {
            ExceptionHelper.CheckCode(this, true, () => {
                if (this.Grid_Books.SelectedRows.Count > 0) {
                    // Удаляем все выбранные записи
                    foreach (DataGridViewRow row in this.Grid_Books.SelectedRows) {
                        // Находим выбранную книгу в БД
                        int selected_id = Convert.ToInt32(row.Cells[0].Value);
                        Book found_entity = DatabaseHelper.SelectFirstOrFormException(
                            DatabaseHelper.db.Books,
                            selected_id
                        );

                        DatabaseHelper.db.Books.Remove(found_entity);
                        DatabaseHelper.db.SaveChanges();
                    }

                    // Обновляем таблицу (также обновит все связанные комбобоксы)
                    this.UpdateData(new List<MetroGrid>() {
                        this.Grid_Books,
                    });

                    // Снимаем выделение в таблице
                    this.Grid_Books.ClearSelection();

                    FormHelper.SendSuccessMessage(this, "Книга успешно удалёна!");
                }
            });
        }

        /// <summary>Событие изменения параметров фильтрации на вкладке "Книги"</summary>
        private void Component_Books_Search_ConditionsChanged(object sender, EventArgs e)
        {
            // Обновляем данные только если установлена галочка "Поиск в реальном времени"
            if (this.CheckBox_Books_Search_IsInRealTime.Checked) {
                this.UpdateData(this.Grid_Books);
            }
        }

        /// <summary>Событие нажатия на кнопку "Поиск" на вкладке "Книги"</summary>
        private void Button_Books_Search_Click(object sender, EventArgs e)
        {
            this.UpdateData(this.Grid_Books);
            this.UnfocusAll();
        }

        /// <summary>Событие нажатия на кнопку "Сбросить фильтр" на вкладке "Книги"</summary>
        private void Button_Books_Search_Reset_Click(object sender, EventArgs e)
        {
            this.RadioButton_Books_Search_Author.Select();
            this.TextBox_Books_Search.Text = "";
            this.UpdateData(this.Grid_Books);
            this.UnfocusAll();
        }

        /// <summary>Событие изменения галочки "Поиск в реальном времени"</summary>
        private void CheckBox_Books_Search_IsInRealTime_CheckedChanged(object sender, EventArgs e)
        {
            // Блокируем кнопку поиска, если включён поиск в реальном времени, и наоборот
            this.Button_Books_Search.Enabled = !this.CheckBox_Books_Search_IsInRealTime.Checked;

            this.UpdateData(this.Grid_Books);
            this.UnfocusAll();
        }
    }
}