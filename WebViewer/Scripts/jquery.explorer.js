$(document).ready(function () {
    var zwsp = '&#8203;';

    // add zero width space to break line
    $('table.viewer span.item').each(function () {
        $(this).attr('title', $(this).text()).html($(this).text().split('').join(zwsp));
    });

    // enable menu on changing view
    var changeViewButton = $('a.changeView');
    changeViewButton.click(function () {
        return false;
    }).contextMenu({
        menu: 'ul#explorerViewMenu',
        isContextMenu: false,
        top: changeViewButton.height(),
        left: 0
    });

    // add watermark to search box
    var watermarkClassName = 'watermark';
    var isSearchBoxSelected = false;
    $('input[type="text"].search').keydown(function (event) {
        if (event.keyCode == 13) {
            var form = $(this);
            while (form.get(0).tagName.toLowerCase() != 'form') {
                form = form.parent();
            }
            form.submit();
        }
    }).focus(function () {
        isSearchBoxSelected = true;
        if ($(this).hasClass(watermarkClassName))
            $(this).removeClass(watermarkClassName).val('');
    }).blur(function () {
        isSearchBoxSelected = false;
        if ($(this).val() == '')
            $(this).addClass(watermarkClassName).val('Search ...');
    }).blur();

    // enable column hover
    $('table.viewer').addClass('columnHover').columnHover();

    // enable menu on navigation bar
    $('span.separator').each(function () {
        var index = parseInt($(this).attr('id'));
        if (index > 0) {
            $(this).css('cursor', 'pointer').contextMenu({
                menu: 'ul#stack_' + index,
                isContextMenu: false,
                top: -parseInt($(this).css('margin-top').replace('px', '')) - 1, // border and margin of UL
                left: $(this).outerWidth()
            });
        }
    });

    // enable context menu on items
    var selectedClassName = 'selected';
    var allRows = $('table.explorer tbody tr');
    var isCtrlDown = false;
    var isShiftDown = false;
    var lastClickTimestamp;
    var lastClicked;
    var checkIfUnselectedAll = function () {
        if ($('table.explorer tbody tr.selected').length == 0) {
            allRows.enableContextMenu();
            lastClicked = undefined;
        }
    };
    $(document).keydown(function (e) {
        if (e.keyCode == 17)
            isCtrlDown = true;
        else if (e.keyCode == 16)
            isShiftDown = true;
        else if (!isSearchBoxSelected && isCtrlDown && (e.keyCode == 65 || e.keyCode == 97)) {
            allRows.addClass(selectedClassName).enableContextMenu();
            return false;
        }
    }).keyup(function (e) {
        if (e.keyCode == 17)
            isCtrlDown = false;
        else if (e.keyCode == 16)
            isShiftDown = false;
    });
    $('table.explorer').each(function () {
        this.onselectstart = function () {
            return !isCtrlDown && !isShiftDown;
        }
    });
    $('table.explorer thead tr').click(function () {
        var selected = $('table.explorer tbody tr.selected');
        if (selected.length > 0) {
            $('table.explorer tbody tr:not(.selected)').enableContextMenu();
            selected.removeClass(selectedClassName);
            lastClicked = undefined;
        }
    });
    $('table.explorer tbody tr td a').click(function (e) {
        if ((e.timeStamp - lastClickTimestamp) < 300)
            window.location = $(this).attr('href');
        else
            lastClickTimestamp = e.timeStamp;
    });
    $('table.explorer:not(.list) tbody tr, table.list tbody tr td a').click(function (e) {
        var tr = $(this).get(0).tagName.toLowerCase() == 'a' ? $(this).parent().parent() : $(this);
        if (lastClicked == undefined)
            allRows.disableContextMenu();

        if (isShiftDown && (lastClicked != undefined)) {
            $('table.explorer tbody tr.selected').removeClass(selectedClassName).disableContextMenu();
            var selectedIndex = allRows.index(lastClicked);
            var clickedIndex = allRows.index(tr);
            var startIndex = Math.min(selectedIndex, clickedIndex);
            var endIndex = Math.max(selectedIndex, clickedIndex);
            for (; startIndex <= endIndex; startIndex++) {
                allRows.eq(startIndex).addClass(selectedClassName).enableContextMenu();
            }
        }
        else if (isCtrlDown) {
            if (tr.hasClass(selectedClassName)) {
                tr.removeClass(selectedClassName).disableContextMenu();
                checkIfUnselectedAll();
            }
            else {
                tr.addClass(selectedClassName).enableContextMenu();
                lastClicked = tr;
            }
        }
        else {
            $('table.explorer tbody tr.selected').removeClass(selectedClassName).disableContextMenu();
            tr.addClass(selectedClassName).enableContextMenu();
            lastClicked = tr;
        }
        return false;
    });
    allRows.contextMenu({
        menu: 'ul#explorerContextMenu',
        isContextMenu: true
    }, function (action, el, pos) {
        switch (action) {
            case "#open":
                window.location = el.find('span.iconText').parent().attr('href');
                break;
            case "#properties":
                var selectedItems = new Array();
                var i = 0;

                var selectedRows = $('table.explorer tbody tr.selected');
                if (selectedRows.length > 0)
                    selectedRows.each(function () {
                        selectedItems[i++] = $(this).find('span.iconText').attr('title');
                    });
                else
                    selectedItems[i] = el.find('span.iconText').attr('title');

                $.ajax({
                    type: "POST",
                    url: "/Home/GetProperties",
                    traditional: true,
                    dataType: "json",
                    data: {
                        machineName: getUrlParameter('machineName'),
                        path: getUrlParameter('path'),
                        selectedItems: selectedItems
                    },
                    success: function (response) {
                        var lenLimit = 40;
                        var items = selectedItems.join(', ');
                        var header = items.length > lenLimit ? (items.substr(0, lenLimit) + ' ...') : items;
                        alert(String.format(
                            '{5}\n\n' +
                            'Location: \n    {0}\n\n' +
                            'Size: \n    {1} ({2} bytes)\n\n' +
                            'Contains: \n    {3} File(s), {4} Folder(s)',
                            getUrlParameter('path'), response.TotalSizeFriendlyString, response.TotalSizeString,
                            response.FileCountString, response.FolderCountString, header));
                    }
                });
                break;
        }
    }, function (el, menu) {
        if (el.find('span.iconText').hasClass('folder'))
            menu.enableContextMenuItems('#open');
        else
            menu.disableContextMenuItems('#open');
    });

    // add explorer selection
    var isSelecting = false;
    var hasCommon = function (l1p1, l1p2, l2p1, l2p2) {
        return (l2p1 < l1p2) && (l2p2 > l1p1);
    };
    function getOffset(el) {
        var _x = 0;
        var _y = 0;
        while (el && !isNaN(el.offsetLeft) && !isNaN(el.offsetTop)) {
            _x += el.offsetLeft - el.scrollLeft;
            _y += el.offsetTop - el.scrollTop;
            el = el.offsetParent;
        }
        return { top: _y, left: _x };
    };
    document.onselectstart = function () {
        return !isSelecting;
    };
    $('div.explorerContainer').mousedown(function (downEvent) {
        if (downEvent.button == 0) {
            $('ul#explorerContextMenu').fadeOut(75);
            allRows.removeClass(selectedClassName);
            checkIfUnselectedAll();

            var container = this;
            if (!isSelecting && (downEvent.pageX <= (container.offsetLeft + container.scrollWidth))
            && (downEvent.pageY <= (container.offsetTop + container.scrollHeight))) {
                isSelecting = true;
                $(container).append('<div class="selection"></div>');
                var borderWidth = parseInt($(container).css("border-left-width"));
                $(document).mousemove(function (moveEvent) {
                    var left = Math.max(Math.min(downEvent.pageX, moveEvent.pageX), container.offsetLeft + borderWidth);
                    var width = Math.min(Math.max(downEvent.pageX, moveEvent.pageX), container.offsetLeft + container.offsetWidth - (borderWidth * 2)) - left - borderWidth;
                    var top = Math.max(Math.min(downEvent.pageY, moveEvent.pageY), container.offsetTop + borderWidth);
                    var height = Math.min(Math.max(downEvent.pageY, moveEvent.pageY), container.offsetTop + container.offsetHeight - (borderWidth * 2)) - top - borderWidth;
                    $('div.selection').css('left', left).css('top', top).css('width', width).css('height', height);
                    $('table.list tbody tr td a').each(function () {
                        var tr = $(this).parent().parent();
                        var offset = getOffset(this);
                        if (hasCommon(offset.left, offset.left + this.offsetWidth, Math.min(downEvent.pageX, moveEvent.pageX), Math.max(downEvent.pageX, moveEvent.pageX))
                        && hasCommon(offset.top, offset.top + this.offsetHeight, Math.min(downEvent.pageY, moveEvent.pageY), Math.max(downEvent.pageY, moveEvent.pageY)))
                            tr.addClass(selectedClassName).enableContextMenu();
                        else
                            tr.removeClass(selectedClassName).disableContextMenu();
                    });
                }).mouseup(function (e) {
                    $('div.selection').remove();
                    isSelecting = false;
                    checkIfUnselectedAll();
                    $(document).unbind('mousemove');
                    $(document).unbind('mouseup');
                });
            }
        }
    });
});

var getUrlParameter = function (paramName) {
    var allParams = document.location.search.substr(1).split('&');
    for (var i = 0; i < allParams.length; i++) {
        var paramPair = allParams[i].split('=');
        if (paramName == decodeURIComponent(paramPair[0])) {
            return decodeURIComponent(paramPair[1]).replace(/\+/g, ' ');
        }
    }
    return '';
};

String.format = function () {
    var s = arguments[0];
    for (var i = 0; i < arguments.length - 1; i++) {
        var reg = new RegExp("\\{" + i + "\\}", "gm");
        s = s.replace(reg, arguments[i + 1]);
    }
    return s;
};

// jQuery Context Menu Plugin
//
// Version 1.01
//
// Cory S.N. LaViska
// A Beautiful Site (http://abeautifulsite.net/)
//
// More info: http://abeautifulsite.net/2008/09/jquery-context-menu-plugin/
//
// Terms of Use
//
// This plugin is dual-licensed under the GNU General Public License
//   and the MIT License and is copyright A Beautiful Site, LLC.
//
var sourceClassName = 'contextMenuSource';
var disabledClassName = 'disabled';
if (jQuery) (function () {
    $.extend($.fn, {

        contextMenu: function (o, callback, onShowing) {
            // Defaults
            if (o.menu == undefined) return false;
            if (o.isContextMenu == undefined) o.isContextMenu = true;
            if (o.top == undefined) o.top = 0;
            if (o.left == undefined) o.left = 0;
            if (o.inSpeed == undefined) o.inSpeed = 150;
            if (o.outSpeed == undefined) o.outSpeed = 75;
            // 0 needs to be -1 for expected results (no fade)
            if (o.inSpeed == 0) o.inSpeed = -1;
            if (o.outSpeed == 0) o.outSpeed = -1;
            // Loop each context menu
            $(this).each(function () {
                var el = $(this);
                var offset = $(el).offset();
                if (!el.hasClass(sourceClassName))
                    el.addClass(sourceClassName);
                // Get this context menu
                var menu = $(o.menu);
                // Add contextMenu class
                $(menu).addClass('contextMenu');
                // Simulate a true right click
                $(this).mousedown(function (e) {
                    var evt = e;
                    evt.stopPropagation();
                    $(this).mouseup(function (e) {
                        e.stopPropagation();
                        var srcElement = $(this);
                        $(this).unbind('mouseup');
                        if ((!o.isContextMenu && evt.button == 0) || (o.isContextMenu && evt.button == 2)) {
                            // Hide context menus that may be showing
                            $(".contextMenu").hide();

                            if ($(el).hasClass(disabledClassName)) return false;

                            // Show the menu
                            if (onShowing) onShowing(el, menu);

                            // Detect mouse position
                            if (o.isContextMenu) {
                                var d = {}, x, y;
                                if (self.innerHeight) {
                                    d.pageYOffset = self.pageYOffset;
                                    d.pageXOffset = self.pageXOffset;
                                    d.innerHeight = self.innerHeight;
                                    d.innerWidth = self.innerWidth;
                                } else if (document.documentElement && document.documentElement.clientHeight) {
                                    d.pageYOffset = document.documentElement.scrollTop;
                                    d.pageXOffset = document.documentElement.scrollLeft;
                                    d.innerHeight = document.documentElement.clientHeight;
                                    d.innerWidth = document.documentElement.clientWidth;
                                } else if (document.body) {
                                    d.pageYOffset = document.body.scrollTop;
                                    d.pageXOffset = document.body.scrollLeft;
                                    d.innerHeight = document.body.clientHeight;
                                    d.innerWidth = document.body.clientWidth;
                                }
                                (e.pageX) ? x = e.pageX : x = e.clientX + d.scrollLeft;
                                (e.pageY) ? y = e.pageY : y = e.clientY + d.scrollTop;

                                $(menu).css({
                                    top: y,
                                    left: x
                                });
                            }
                            else {
                                $(menu).css({
                                    top: offset.top + o.top,
                                    left: offset.left + o.left
                                });
                            }

                            $(document).unbind('click');
                            $(menu).fadeIn(o.inSpeed);
                            // Hover events
                            $(menu).find('A').mouseover(function () {
                                $(menu).find('LI.hover').removeClass('hover');
                                $(this).parent().addClass('hover');
                            }).mouseout(function () {
                                $(menu).find('LI.hover').removeClass('hover');
                            });

                            // Keyboard
                            $(document).keypress(function (e) {
                                switch (e.keyCode) {
                                    case 38: // up
                                        if ($(menu).find('LI.hover').size() == 0) {
                                            $(menu).find('LI:last').addClass('hover');
                                        } else {
                                            $(menu).find('LI.hover').removeClass('hover').prevAll('LI:not(.disabled)').eq(0).addClass('hover');
                                            if ($(menu).find('LI.hover').size() == 0) $(menu).find('LI:last').addClass('hover');
                                        }
                                        break;
                                    case 40: // down
                                        if ($(menu).find('LI.hover').size() == 0) {
                                            $(menu).find('LI:first').addClass('hover');
                                        } else {
                                            $(menu).find('LI.hover').removeClass('hover').nextAll('LI:not(.disabled)').eq(0).addClass('hover');
                                            if ($(menu).find('LI.hover').size() == 0) $(menu).find('LI:first').addClass('hover');
                                        }
                                        break;
                                    case 13: // enter
                                        $(menu).find('LI.hover A').trigger('click');
                                        break;
                                    case 27: // esc
                                        $(document).trigger('click');
                                        break
                                }
                            });

                            // When items are selected
                            $(menu).find('A').unbind('click');
                            $(menu).find('LI A').click(function () {
                                if ($(this).parent().hasClass(disabledClassName))
                                    return false;

                                $(document).unbind('click').unbind('keypress');
                                $(".contextMenu").hide();
                                // Callback
                                if (callback) {
                                    callback($(this).attr('href'), $(srcElement), { x: x - offset.left, y: y - offset.top, docX: x, docY: y });
                                    return false;
                                }
                                else
                                    return true;
                            });

                            // Hide bindings
                            setTimeout(function () { // Delay for Mozilla
                                $(document).click(function () {
                                    $(document).unbind('click').unbind('keypress');
                                    $(menu).fadeOut(o.outSpeed);
                                    return false;
                                });
                            }, 0);
                        }
                    });
                });

                // Disable text selection
                if ($.browser.mozilla) {
                    $(menu).each(function () { $(this).css({ 'MozUserSelect': 'none' }); });
                } else if ($.browser.msie) {
                    $(menu).each(function () { $(this).bind('selectstart.disableTextSelect', function () { return false; }); });
                } else {
                    $(menu).each(function () { $(this).bind('mousedown.disableTextSelect', function () { return false; }); });
                }

                // Disable browser context menu (requires both selectors to work in IE/Safari + FF/Chrome)
                if (o.isContextMenu)
                    $(el).add($('UL.contextMenu')).bind('contextmenu', function () {
                        return el.hasClass(disabledClassName);
                    });

            });
            return $(this);
        },

        // Disable context menu items on the fly
        disableContextMenuItems: function (o) {
            if (o == undefined) {
                // Disable all
                $(this).find('LI').addClass(disabledClassName);
                return ($(this));
            }
            $(this).each(function () {
                if (o != undefined) {
                    var d = o.split(',');
                    for (var i = 0; i < d.length; i++) {
                        $(this).find('A[href="' + d[i] + '"]').parent().addClass(disabledClassName);

                    }
                }
            });
            return ($(this));
        },

        // Enable context menu items on the fly
        enableContextMenuItems: function (o) {
            if (o == undefined) {
                // Enable all
                $(this).find('LI.disabled').removeClass(disabledClassName);
                return ($(this));
            }
            $(this).each(function () {
                if (o != undefined) {
                    var d = o.split(',');
                    for (var i = 0; i < d.length; i++) {
                        $(this).find('A[href="' + d[i] + '"]').parent().removeClass(disabledClassName);

                    }
                }
            });
            return ($(this));
        },

        // Disable context menu(s)
        disableContextMenu: function () {
            $(this).each(function () {
                $(this).addClass(disabledClassName);
            });
            return ($(this));
        },

        // Enable context menu(s)
        enableContextMenu: function () {
            $(this).each(function () {
                $(this).removeClass(disabledClassName);
            });
            return ($(this));
        },

        // Destroy context menu(s)
        destroyContextMenu: function () {
            // Destroy specified context menus
            $(this).each(function () {
                // Disable action
                $(this).removeClass(sourceClassName);
                $(this).unbind('mousedown').unbind('mouseup');
            });
            return ($(this));
        },

        isContextMenuBuilt: function () {
            return $(this).hasClass(sourceClassName);
        }

    });
})(jQuery);

/*
* jQuery columnHover plugin
* Version: 0.1.1
*
* Copyright (c) 2007 Roman Weich
* http://p.sohei.org
*
* Dual licensed under the MIT and GPL licenses 
* (This means that you can choose the license that best suits your project, and use it accordingly):
*   http://www.opensource.org/licenses/mit-license.php
*   http://www.gnu.org/licenses/gpl.html
*
* Changelog: 
* v 0.1.1 - 2007-08-05
*	-change: included new option "ignoreCols", through which columns can be excluded from the highlighting process
* v 0.1.0 - 2007-05-25
*/

(function ($) {
    /**
    * Calculates the actual cellIndex value of all cells in the table and stores it in the realCell property of each cell.
    * Thats done because the cellIndex value isn't correct when colspans or rowspans are used.
    * Originally created by Matt Kruse for his table library - Big Thanks! (see http://www.javascripttoolbox.com/)
    * @param {element} table	The table element.
    */
    var fixCellIndexes = function (table) {
        var rows = table.rows;
        var len = rows.length;
        var matrix = [];
        for (var i = 0; i < len; i++) {
            var cells = rows[i].cells;
            var clen = cells.length;
            for (var j = 0; j < clen; j++) {
                var c = cells[j];
                var rowSpan = c.rowSpan || 1;
                var colSpan = c.colSpan || 1;
                var firstAvailCol = -1;
                if (!matrix[i]) {
                    matrix[i] = [];
                }
                var m = matrix[i];
                // Find first available column in the first row
                while (m[++firstAvailCol]) { }
                c.realIndex = firstAvailCol;
                for (var k = i; k < i + rowSpan; k++) {
                    if (!matrix[k]) {
                        matrix[k] = [];
                    }
                    var matrixrow = matrix[k];
                    for (var l = firstAvailCol; l < firstAvailCol + colSpan; l++) {
                        matrixrow[l] = 1;
                    }
                }
            }
        }
    };

    /**
    * Highlight whole table columns when hovering over a table.
    * Works on tables with rowspans and colspans.
    *
    * @param {map} options			An object for optional settings (options described below).
    *
    * @option {string} hoverClass		A CSS class that is set on the cells in the column with the mouse over.
    *							Default value: 'hover'
    * @option {boolean} eachCell		Allows highlighting the column while hovering over the table body or table footer. When disabled, highlighting is allowed only through the table header.
    *							Default value: false
    * @option {boolean} includeSpans		Includes columns with the colspan attribute set in the hover and highlight process.
    *							Default value: true
    * @option {array} ignoreCols		An array of numbers. Each column with the matching column index won't be included in the highlighting process.
    *							Index starting at 1!
    *							Default value: [] (empty array)
    *
    * @example $('#table').columnHover();
    * @desc Allow column hovering/highlighting for the table using the default settings.
    *
    * @example $('#table').columnHover({eachCell:true, hoverClass:'someclass'});
    * @desc Allow column hovering/highlighting for the whole table (including the body and footer). Set the class "someclass" to the cells in the column with the mouse over.
    *
    * @type jQuery
    *
    * @name columnHover
    * @cat Plugins/columnHover
    * @author Roman Weich (http://p.sohei.org)
    */
    $.fn.columnHover = function (options) {
        var settings = $.extend({
            hoverClass: 'hover',
            eachCell: false,
            includeSpans: true,
            ignoreCols: []
        }, options);

        /**
        * Adds or removes the hover style on the column.
        * @param {element} cell	The cell with the mouseover/mouseout event.
        * @param {array} colIndex	The index with the stored columns.
        * @param {boolean} on		Defines whether the style will be set or removed.
        */
        var hover = function (cell, colIndex, on) {
            var a = colIndex[cell.realIndex];
            var i = 0;
            if ($(settings.ignoreCols).index(cell.realIndex + 1) != -1) {
                return; //dont highlight the columns in the ignoreCols array
            }
            while (++i < cell.colSpan) {
                a = a.concat(colIndex[cell.realIndex + i]);
            }
            if (on) {
                $(a).addClass(settings.hoverClass);
            }
            else {
                $(a).removeClass(settings.hoverClass);
            }
        };

        /**
        * Adds the hover events to the cell.
        * @param {jQuery result array} $s	The elements to add the events to.
        * @param {array} colIndex	The index with the stored columns.
        */
        var addHover = function ($s, colIndex) {
            $s.bind('mouseover', function () {
                hover(this, colIndex, true);
            }).bind('mouseout', function () {
                hover(this, colIndex, false);
            });
        };

        return this.each(function () {
            var colIndex = [];
            var tbl = this;
            var body, row, c, tboI, rowI, cI, rI, s;

            if (!tbl.tBodies || !tbl.tBodies.length || !tbl.tHead || !settings.hoverClass.length) {
                return;
            }
            fixCellIndexes(tbl);
            //create index - loop through the bodies
            for (tboI = 0; tboI < tbl.tBodies.length; tboI++) {
                body = tbl.tBodies[tboI];
                //loop through the rows
                for (rowI = 0; rowI < body.rows.length; rowI++) {
                    row = body.rows[rowI];
                    //each cell
                    for (cI = 0; cI < row.cells.length; cI++) {
                        c = row.cells[cI];
                        //ignore cells with colspan?
                        if (!settings.includeSpans && c.colSpan > 1) {
                            continue;
                        }
                        s = (settings.includeSpans) ? c.colSpan : 1;
                        while (--s >= 0) {
                            rI = c.realIndex + s;
                            if (!colIndex[rI]) {
                                colIndex[rI] = [];
                            }
                            colIndex[rI].push(c);
                        }
                        //add hover event?
                        if (settings.eachCell) {
                            addHover($(c), colIndex);
                        }
                    }
                }
            }
            //events
            addHover($('td, th', tbl.tHead), colIndex);
            //add hover event to footer?
            if (settings.eachCell && tbl.tFoot) {
                addHover($('td, th', tbl.tFoot), colIndex);
            }
        });
    };
})(jQuery); 
