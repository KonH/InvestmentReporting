<template>
	<h1>Import</h1>
	<div class="form-group">
		<label>
			Importer:
			<select ref="importer" class="form-control">
				<option>AlphaDirectMyBroker</option>
				<option>TinkoffBrokerReport</option>
			</select>
		</label>
	</div>
	<div class="form-group">
		<label>
			File:
			<input ref="file" type="file" class="form-control-file" />
		</label>
	</div>
	<button :onclick="onclick" class="btn btn-primary">Import</button>
	<router-link to="/" class="btn btn-secondary ml-2">Back</router-link>
</template>
<script lang="ts">
import { Options, Vue } from 'vue-class-component';
import Backend from '@/service/backend';
import router from '@/router';
import { Action } from 'vuex-class';
import { Ref } from 'vue-property-decorator';

@Options({
	name: 'Import',
})
export default class Import extends Vue {
	@Action('fetchActiveState')
	fetchActiveState!: () => void;

	@Ref('importer')
	importerInput!: HTMLInputElement;

	@Ref('file')
	fileInput!: HTMLInputElement;

	async onclick() {
		const brokerId = this.$route.params.broker as string;
		const report = this.fileInput.files != null ? this.fileInput.files[0] : null;
		if (!report) {
			alert('No file selected');
			return;
		}
		const result = await Backend.tryFetch(
			Backend.import().import.importCreate(
				{
					date: new Date().toISOString(),
					broker: brokerId,
					importer: this.importerInput.value,
				},
				{
					report: report,
				}
			)
		);
		if (result?.ok) {
			this.fetchActiveState();
			await router.push('/');
		} else {
			alert(`Failed: ${result?.error}`);
		}
	}
}
</script>
