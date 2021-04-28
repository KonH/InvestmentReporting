<template>
	<div class="form-group">
		<label>
			Broker:
			<select ref="broker" class="form-control">
				<option v-for="broker in brokers" :key="broker.id" :value="broker.id">{{ broker.displayName }}</option>
			</select>
		</label>
	</div>
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
	<button :onclick="onclick" :class="buttonClass">Import</button>
</template>
<script lang="ts">
import { Options, Vue } from 'vue-class-component';
import Backend from '@/service/backend';
import router from '@/router';
import { Action, State } from 'vuex-class';
import { Ref } from 'vue-property-decorator';
import { StateDto } from '@/api/state';
import Progress from '@/utils/progress';

@Options({
	name: 'ImportView',
})
export default class ImportView extends Vue {
	@State('activeState')
	activeState!: StateDto;

	@Action('fetchState')
	fetchState!: () => void;

	@Ref('broker')
	brokerInput!: HTMLInputElement;

	@Ref('importer')
	importerInput!: HTMLInputElement;

	@Ref('file')
	fileInput!: HTMLInputElement;

	isInProgress = false;

	get brokers() {
		return this.activeState.brokers;
	}

	get buttonClass() {
		return Progress.getClass(this, 'btn btn-primary');
	}

	async onclick() {
		await Progress.wrap(this, this.onclickApply);
	}

	async onclickApply() {
		const brokerId = this.brokerInput.value;
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
		if (result.ok) {
			await Backend.tryFetch(Backend.market().sync.syncCreate());
			this.fetchState();
			await router.push('/');
		} else {
			alert(`Failed: ${result.statusText}`);
		}
	}
}
</script>
