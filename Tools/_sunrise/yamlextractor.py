import os

from fluent.syntax.parser import FluentParser
from fluent.syntax.serializer import FluentSerializer

from file import YAMLFile, FluentFile
from fluentast import FluentSerializedMessage, FluentAstAttributeFactory
from fluentformatter import FluentFormatter
from project import Project
import logging

######################################### Class defifitions ############################################################
class YAMLExtractor:
    def __init__(self, yaml_files):
        self.yaml_files = yaml_files

    def execute(self):
        for yaml_file in yaml_files:
            yaml_elements = yaml_file.get_elements(yaml_file.parse_data(yaml_file.read_data()))

            if not len(yaml_elements):
                continue

            fluent_file_serialized = self.get_serialized_fluent_from_yaml_elements(yaml_elements)

            if not fluent_file_serialized:
                continue

            pretty_fluent_file_serialized = formatter.format_serialized_file_data(fluent_file_serialized)

            relative_parent_dir = yaml_file.get_relative_parent_dir(project.prototypes_dir_path).lower()
            file_name = yaml_file.get_name()

            en_fluent_file_path = self.create_en_fluent_file(relative_parent_dir, file_name, pretty_fluent_file_serialized)
            ru_fluent_file_path = self.create_ru_fluent_file(en_fluent_file_path)

    def get_serialized_fluent_from_yaml_elements(self, yaml_elements):
        fluent_serialized_messages = []

        for el in yaml_elements:
            if isinstance(el.parent_id, list):
                el.parent_id = el.parent_id[0]

            fluent_message = FluentSerializedMessage.from_yaml_element(el.id, el.name, FluentAstAttributeFactory.from_yaml_element(el), el.parent_id)
            if fluent_message:
                fluent_serialized_messages.append(fluent_message)

        if not fluent_serialized_messages:
            return None

        return '\n'.join(fluent_serialized_messages)

    def create_en_fluent_file(self, relative_parent_dir, file_name, file_data):
        en_new_dir_path = os.path.join(project.en_locale_prototypes_dir_path, relative_parent_dir)

        os.makedirs(en_new_dir_path, exist_ok=True)

        en_fluent_file_path = os.path.join(en_new_dir_path, f'{file_name}.ftl')

        en_fluent_file = FluentFile(en_fluent_file_path)
        en_fluent_file.save_data(file_data)

        logging.info(f'Актуализирован файл английской локализации {en_fluent_file.full_path}')

        return en_fluent_file.full_path

    def create_ru_fluent_file(self, en_analog_file_path):
        ru_file_full_path = en_analog_file_path.replace('en-US', 'ru-RU')

        os.makedirs(os.path.dirname(ru_file_full_path), exist_ok=True)

        if os.path.isfile(ru_file_full_path):
            return
        else:
            en_file = FluentFile(f'{en_analog_file_path}')
            file = FluentFile(f'{ru_file_full_path}')
            file.save_data(en_file.read_data())
            logging.info(f'Создан файл русской локализации {ru_file_full_path}')

        return ru_file_full_path


######################################## Var definitions ###############################################################

logging.basicConfig(level = logging.INFO)
project = Project()
serializer = FluentSerializer()
parser = FluentParser()
formatter = FluentFormatter()

logging.info(f'Поиск YAML-файлов ...')
yaml_files_paths = project.get_files_paths_by_dir(project.prototypes_dir_path, 'yml')
if not yaml_files_paths:
    logging.info("Не найдено YAML файлов!")
else:
    logging.info(f"Найдено {len(yaml_files_paths)} YAML файлов. Обработка...")
yaml_files = list(map(lambda yaml_file_path: YAMLFile(yaml_file_path), yaml_files_paths))

########################################################################################################################

YAMLExtractor(yaml_files).execute()
